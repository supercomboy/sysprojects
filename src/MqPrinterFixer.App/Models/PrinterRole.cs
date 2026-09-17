namespace MqPrinterFixer.App.Models;

/// <summary>
/// Vai trò máy in của máy hiện tại (Master Prompt Section 8).
/// </summary>
public enum PrinterRole
{
    /// <summary>Có printer local nhưng không share, không dùng shared printer từ máy khác.</summary>
    Standalone = 0,

    /// <summary>Có ít nhất một printer local đang được share (Shared=true + ShareName).</summary>
    Host,

    /// <summary>Sử dụng ít nhất một shared printer từ máy khác (qua UNC path).</summary>
    Client,

    /// <summary>Vừa share printer local, vừa kết nối tới shared printer của máy khác.</summary>
    Hybrid
}