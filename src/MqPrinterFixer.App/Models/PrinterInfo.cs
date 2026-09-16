namespace MqPrinterFixer.App.Models;

/// <summary>
/// Thông tin máy in (read-only, snapshot từ WMI).
/// </summary>
public sealed record PrinterInfo(
    string Name,
    PrinterStatus Status,
    bool IsDefault,
    PrinterConnectionType ConnectionType,
    string PortName,
    string DriverName,
    bool IsShared,
    string ShareName,
    string SourceHost,
    string PrinterPath,
    string Location,
    string Comment)
{
    /// <summary>
    /// True nếu máy in này được kết nối từ máy khác (role = Client cho printer này).
    /// </summary>
    public bool IsRemote => !string.IsNullOrWhiteSpace(SourceHost) &&
                            !SourceHost.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
}