using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

/// <summary>
/// Quản lý theme hiện tại và áp dụng vào WPF ResourceDictionary ở runtime.
/// </summary>
public interface IThemeService
{
    /// <summary>Theme người dùng đã chọn (có thể là System).</summary>
    AppTheme CurrentTheme { get; }

    /// <summary>Theme thực tế đang áp dụng (System đã resolve thành Light/Dark).</summary>
    AppTheme EffectiveTheme { get; }

    /// <summary>Phát khi theme thay đổi (kể cả khi System theme bị Windows đổi).</summary>
    event EventHandler? ThemeChanged;

    /// <summary>Nạp theme lần đầu khi app khởi động.</summary>
    void Initialize();

    /// <summary>Áp dụng theme mới ngay lập tức (không cần restart).</summary>
    void ApplyTheme(AppTheme theme);
}