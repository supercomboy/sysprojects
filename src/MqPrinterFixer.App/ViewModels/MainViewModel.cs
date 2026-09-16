using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private string _appName = "MQ Printer Fixer";

    [ObservableProperty]
    private string _subtitle = "Diagnose and repair LAN printer problems";

    [ObservableProperty]
    private string _statusMessage = "Phase 5 — Theme system ready";

    [ObservableProperty]
    private string _currentThemeDisplay = string.Empty;

    public MainViewModel(IThemeService themeService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        Title = AppName;

        _themeService.ThemeChanged += OnThemeChanged;
        UpdateThemeDisplay();
    }

    /// <summary>
    /// Nút tạm để test runtime theme swap trong Phase 5.
    /// Sẽ được thay bằng Settings page ở Phase 23.
    /// </summary>
    [RelayCommand]
    private void CycleTheme()
    {
        var next = _themeService.CurrentTheme switch
        {
            AppTheme.System => AppTheme.Light,
            AppTheme.Light => AppTheme.Dark,
            AppTheme.Dark => AppTheme.System,
            _ => AppTheme.System
        };
        _themeService.ApplyTheme(next);
    }

    private void OnThemeChanged(object? sender, EventArgs e) => UpdateThemeDisplay();

    private void UpdateThemeDisplay()
    {
        CurrentThemeDisplay = _themeService.CurrentTheme switch
        {
            AppTheme.System => $"System ({_themeService.EffectiveTheme})",
            _ => _themeService.CurrentTheme.ToString()
        };
    }
}