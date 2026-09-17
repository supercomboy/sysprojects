namespace MqPrinterFixer.App.Models;

/// <summary>
/// Kết quả dò Printer Role.
/// DetectedRole là kết quả thật từ danh sách máy in.
/// EffectiveRole là kết quả sau khi áp Manual Override.
/// </summary>
public sealed record PrinterRoleDetectionResult(
    PrinterRole DetectedRole,
    PrinterRole EffectiveRole,
    RoleDetectionMode Mode,
    int TotalPrinterCount,
    int LocalSharedPrinterCount,
    int RemoteSharedPrinterCount);