using System.Management;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class PrinterService : IPrinterService
{
    private const string WmiQuery = "SELECT * FROM Win32_Printer";

    public Task<IReadOnlyList<PrinterInfo>> GetAllPrintersAsync(CancellationToken cancellationToken = default)
        => Task.Run<IReadOnlyList<PrinterInfo>>(() => QueryPrinters(cancellationToken), cancellationToken);

    public async Task<PrinterInfo?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default)
    {
        var all = await GetAllPrintersAsync(cancellationToken);
        return all.FirstOrDefault(p => p.IsDefault);
    }

    private static IReadOnlyList<PrinterInfo> QueryPrinters(CancellationToken cancellationToken)
    {
        var list = new List<PrinterInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher(WmiQuery);
            using var collection = searcher.Get();

            foreach (ManagementBaseObject obj in collection)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (obj)
                {
                    var info = MapPrinter(obj);
                    if (info is not null)
                    {
                        list.Add(info);
                    }
                }
            }
        }
        catch (ManagementException)
        {
            // WMI không available hoặc query fail. Trả list rỗng.
        }
        catch (UnauthorizedAccessException)
        {
            // Không có quyền WMI read (hiếm, nhưng có thể bị group policy chặn).
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // WMI service bị dừng hoặc không phản hồi.
        }

        return list;
    }

    private static PrinterInfo? MapPrinter(ManagementBaseObject obj)
    {
        var name = ReadString(obj, "Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var isDefault      = ReadBool(obj, "Default");
        var workOffline    = ReadBool(obj, "WorkOffline");
        var isShared       = ReadBool(obj, "Shared");
        var portName       = ReadString(obj, "PortName");
        var driverName     = ReadString(obj, "DriverName");
        var shareName      = ReadString(obj, "ShareName");
        var serverName     = ReadString(obj, "ServerName");
        var location       = ReadString(obj, "Location");
        var comment        = ReadString(obj, "Comment");

        var printerStatus   = ReadInt(obj, "PrinterStatus");
        var detectedError   = ReadInt(obj, "DetectedErrorState");

        var connectionType = PrinterConnectionClassifier.Classify(portName, name);
        var status = PrinterStatusMapper.Map(workOffline, printerStatus, detectedError);

        var (sourceHost, printerPath) = ComputeSourceAndPath(
            connectionType, serverName, shareName, isShared);

        return new PrinterInfo(
            Name: name,
            Status: status,
            IsDefault: isDefault,
            ConnectionType: connectionType,
            PortName: portName,
            DriverName: driverName,
            IsShared: isShared,
            ShareName: shareName,
            SourceHost: sourceHost,
            PrinterPath: printerPath,
            Location: location,
            Comment: comment);
    }

    private static (string SourceHost, string PrinterPath) ComputeSourceAndPath(
        PrinterConnectionType type,
        string serverName,
        string shareName,
        bool isShared)
    {
        var localMachine = Environment.MachineName;

        // Network printer from another machine → SourceHost = serverName
        if (type == PrinterConnectionType.NetworkShared &&
            !string.IsNullOrWhiteSpace(serverName) &&
            !serverName.Equals(localMachine, StringComparison.OrdinalIgnoreCase))
        {
            var path = string.IsNullOrWhiteSpace(shareName)
                ? string.Empty
                : $@"\\{serverName}\{shareName}";

            return (serverName, path);
        }

        // Local shared printer → SourceHost = local machine
        if (isShared && !string.IsNullOrWhiteSpace(shareName))
        {
            return (localMachine, $@"\\{localMachine}\{shareName}");
        }

        return (string.Empty, string.Empty);
    }

    private static string ReadString(ManagementBaseObject obj, string property)
    {
        try
        {
            return obj[property] as string ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool ReadBool(ManagementBaseObject obj, string property)
    {
        try
        {
            var value = obj[property];
            return value is bool b && b;
        }
        catch
        {
            return false;
        }
    }

    private static int? ReadInt(ManagementBaseObject obj, string property)
    {
        try
        {
            var value = obj[property];
            if (value is null)
            {
                return null;
            }

            return Convert.ToInt32(value);
        }
        catch
        {
            return null;
        }
    }
}