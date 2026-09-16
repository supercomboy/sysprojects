using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public enum RenameDialogStage
{
    Input,
    Preview,
    Result
}

public sealed partial class RenameComputerDialogViewModel : ViewModelBase
{
    private readonly IComputerService _computerService;
    private readonly ISystemInfoService _systemInfo;

    [ObservableProperty]
    private string _currentName = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _validationError = string.Empty;

    [ObservableProperty]
    private bool _isNewNameValid;

    [ObservableProperty]
    private bool _isAdministrator;

    [ObservableProperty]
    private RenameDialogStage _stage = RenameDialogStage.Input;

    [ObservableProperty]
    private string _resultMessage = string.Empty;

    [ObservableProperty]
    private bool _renameSucceeded;

    [ObservableProperty]
    private bool _restartRequired;

    [ObservableProperty]
    private bool _isApplying;

    /// <summary>Phát khi user bấm Close hoặc khi apply thành công và muốn đóng dialog.</summary>
    public event EventHandler<bool>? CloseRequested;

    public RenameComputerDialogViewModel(
        IComputerService computerService,
        ISystemInfoService systemInfo)
    {
        _computerService = computerService ?? throw new ArgumentNullException(nameof(computerService));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));

        Title = "Rename Computer";
        _ = LoadCurrentAsync();
    }

    partial void OnNewNameChanged(string value)
    {
        var v = HostnameValidator.Validate(value, CurrentName);
        IsNewNameValid = v.IsValid;
        ValidationError = v.IsValid ? string.Empty : v.ErrorMessage;
    }

    [RelayCommand]
    private async Task LoadCurrentAsync()
    {
        try
        {
            var info = await _systemInfo.GetComputerInfoAsync();
            CurrentName = info.ComputerName;
            IsAdministrator = info.IsAdministrator;
        }
        catch
        {
            CurrentName = "(unknown)";
            IsAdministrator = false;
        }
    }

    [RelayCommand]
    private void GoToPreview()
    {
        if (!IsNewNameValid)
        {
            return;
        }
        Stage = RenameDialogStage.Preview;
    }

    [RelayCommand]
    private void BackToInput()
    {
        Stage = RenameDialogStage.Input;
        ResultMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (IsApplying)
        {
            return;
        }

        IsApplying = true;
        try
        {
            var result = await _computerService.RenameComputerAsync(NewName);

            RenameSucceeded = result.Success;
            RestartRequired = result.RestartRequired;
            ResultMessage = result.Success
                ? $"Đổi tên thành công: {result.OldName} → {result.NewName}.\n\nKhởi động lại máy để áp dụng."
                : $"Đổi tên thất bại.\n\n{result.ErrorMessage}";

            Stage = RenameDialogStage.Result;
        }
        finally
        {
            IsApplying = false;
        }
    }

    [RelayCommand]
    private async Task RestartNowAsync()
    {
        await _computerService.RestartComputerNowAsync();
        RequestClose(true);
    }

    [RelayCommand]
    private void RestartLater()
    {
        RequestClose(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose(false);
    }

    private void RequestClose(bool refreshParent)
    {
        CloseRequested?.Invoke(this, refreshParent);
    }
}