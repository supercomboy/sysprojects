using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Pure logic xác định PrinterRole từ danh sách máy in (Master Prompt Section 8).
/// Không phụ thuộc WMI, không phụ thuộc Environment — testable 100%.
/// </summary>
public static class PrinterRoleMapper
{
    /// <summary>
    /// Tính role thật từ danh sách máy in.
    /// </summary>
    public static PrinterRole ComputeRole(
        IReadOnlyList<PrinterInfo> printers,
        string localMachineName)
    {
        var local = localMachineName ?? string.Empty;

        var hasHost = false;
        var hasClient = false;

        foreach (var p in printers)
        {
            if (IsRemoteShared(p, local))
            {
                hasClient = true;
            }
            else if (IsLocalShared(p))
            {
                hasHost = true;
            }

            if (hasHost && hasClient)
            {
                break;
            }
        }

        return (hasHost, hasClient) switch
        {
            (true, true)  => PrinterRole.Hybrid,
            (true, false) => PrinterRole.Host,
            (false, true) => PrinterRole.Client,
            (false, false) => PrinterRole.Standalone
        };
    }

    /// <summary>
    /// Áp Manual Override. Auto giữ nguyên giá trị đã dò.
    /// </summary>
    public static PrinterRole ApplyOverride(PrinterRole detected, RoleDetectionMode mode) => mode switch
    {
        RoleDetectionMode.Auto        => detected,
        RoleDetectionMode.ForceHost   => PrinterRole.Host,
        RoleDetectionMode.ForceClient => PrinterRole.Client,
        RoleDetectionMode.ForceHybrid => PrinterRole.Hybrid,
        _ => detected
    };

    /// <summary>
    /// Máy in này là local shared đúng nghĩa — cơ sở để xác định Host.
    /// YÊU CẦU: Shared=true AND ShareName non-empty AND không phải remote.
    /// USB printer một mình KHÔNG đủ điều kiện.
    /// </summary>
    public static bool IsLocalShared(PrinterInfo printer)
    {
        if (printer is null) return false;
        if (!printer.IsShared) return false;
        if (string.IsNullOrWhiteSpace(printer.ShareName)) return false;
        return true;
    }

    /// <summary>
    /// Máy in này đến từ máy khác — cơ sở để xác định Client.
    /// </summary>
    public static bool IsRemoteShared(PrinterInfo printer, string localMachineName)
    {
        if (printer is null) return false;
        if (string.IsNullOrWhiteSpace(printer.SourceHost)) return false;

        var local = localMachineName ?? string.Empty;
        return !printer.SourceHost.Equals(local, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Nhãn hiển thị UI cho PrinterRole.</summary>
    public static string ToDisplayName(PrinterRole role) => role switch
    {
        PrinterRole.Standalone => "Standalone",
        PrinterRole.Host       => "Printer Host",
        PrinterRole.Client     => "Printer Client",
        PrinterRole.Hybrid     => "Host + Client",
        _                      => "Unknown"
    };

    /// <summary>Nhãn hiển thị UI cho RoleDetectionMode.</summary>
    public static string ToDisplayName(RoleDetectionMode mode) => mode switch
    {
        RoleDetectionMode.Auto        => "Auto Detect",
        RoleDetectionMode.ForceHost   => "Printer Host",
        RoleDetectionMode.ForceClient => "Printer Client",
        RoleDetectionMode.ForceHybrid => "Host + Client",
        _                             => "Auto Detect"
    };
}