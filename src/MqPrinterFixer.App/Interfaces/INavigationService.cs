using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.ViewModels;

namespace MqPrinterFixer.App.Interfaces;

/// <summary>
/// Quản lý page hiện tại và điều hướng giữa các page.
/// </summary>
public interface INavigationService
{
    /// <summary>ViewModel của page đang hiển thị ở vùng Main Content.</summary>
    PageViewModelBase? CurrentPage { get; }

    /// <summary>Phát khi page thay đổi.</summary>
    event EventHandler? CurrentPageChanged;

    /// <summary>Điều hướng tới page tương ứng với <paramref name="key"/>.</summary>
    void Navigate(NavigationItemKey key);

    /// <summary>Điều hướng tới Dashboard (page mặc định).</summary>
    void NavigateToDefault();
}