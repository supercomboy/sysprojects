using CommunityToolkit.Mvvm.ComponentModel;

namespace MqPrinterFixer.App.ViewModels;

/// <summary>
/// Base class cho mọi ViewModel trong ứng dụng.
/// Kế thừa <see cref="ObservableObject"/> từ CommunityToolkit.Mvvm để có sẵn
/// cơ chế PropertyChanged + SetProperty.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _busyMessage = string.Empty;
}