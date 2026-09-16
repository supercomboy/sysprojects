namespace MqPrinterFixer.App.Models;

/// <summary>
/// Trạng thái chuẩn hóa của máy in (Master Prompt Section 27).
/// </summary>
public enum PrinterStatus
{
    Unknown = 0,
    Ready,
    Offline,
    Paused,
    Error
}