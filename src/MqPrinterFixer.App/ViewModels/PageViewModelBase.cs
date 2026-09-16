using CommunityToolkit.Mvvm.ComponentModel;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

/// <summary>
/// Base class cho mọi page ViewModel. Kế thừa ViewModelBase
/// để có sẵn Title, IsBusy, BusyMessage.
/// </summary>
public abstract partial class PageViewModelBase : ViewModelBase
{
    /// <summary>Định danh page – dùng để highlight trong sidebar.</summary>
    public abstract NavigationItemKey Key { get; }

    /// <summary>Mô tả ngắn hiển thị trong Content area ở Phase 6.</summary>
    [ObservableProperty]
    private string _description = string.Empty;
}