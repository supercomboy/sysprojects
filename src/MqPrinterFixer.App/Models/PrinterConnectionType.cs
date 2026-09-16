namespace MqPrinterFixer.App.Models;

/// <summary>
/// Kiểu kết nối máy in (Master Prompt Section 26).
/// </summary>
public enum PrinterConnectionType
{
    Unknown = 0,
    Usb,
    TcpIp,
    Wsd,
    NetworkShared,
    Virtual
}