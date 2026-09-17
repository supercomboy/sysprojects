namespace MqPrinterFixer.App.Models;

/// <summary>
/// Chế độ xác định Printer Role (Master Prompt Section 10).
/// Manual Override chỉ ảnh hưởng cách app sử dụng role logic,
/// KHÔNG tự động thay đổi cấu hình Windows printer.
/// </summary>
public enum RoleDetectionMode
{
    /// <summary>Mặc định — tự động dò từ danh sách máy in.</summary>
    Auto = 0,

    /// <summary>Ép hiển thị như Printer Host.</summary>
    ForceHost,

    /// <summary>Ép hiển thị như Printer Client.</summary>
    ForceClient,

    /// <summary>Ép hiển thị như Host + Client.</summary>
    ForceHybrid
}