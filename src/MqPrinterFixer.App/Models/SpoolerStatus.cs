namespace MqPrinterFixer.App.Models;

/// <summary>
/// Trạng thái chuẩn hóa của Print Spooler (Master Prompt Section 37).
/// </summary>
public enum SpoolerStatus
{
    Unknown = 0,
    Running,
    Stopped,
    Starting,
    Stopping,
    Paused
}