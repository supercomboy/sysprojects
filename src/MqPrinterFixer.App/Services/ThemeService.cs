using System.Windows;
using Microsoft.Win32;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class ThemeService : IThemeService
{
    // Pack URI tới file ResourceDictionary trong assembly hiện tại.
    private const string LightPackUri =
        "pack://application:,,,/MqPrinterFixer.App;component/Themes/LightTheme.xaml";

    private const string DarkPackUri =
        "pack://application:,,,/MqPrinterFixer.App;component/Themes/DarkTheme.xaml";

    private AppTheme _currentTheme = AppTheme.System;

    public AppTheme CurrentTheme => _currentTheme;

    public AppTheme EffectiveTheme => ResolveEffectiveTheme(_currentTheme);

    public event EventHandler? ThemeChanged;

    public void Initialize()
    {
        // Áp dụng theme lần đầu (resolve System → Light/Dark)
        ApplyTheme(_currentTheme);
    }

    public void ApplyTheme(AppTheme theme)
    {
        _currentTheme = theme;
        SwapThemeDictionary(EffectiveTheme);
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private static AppTheme ResolveEffectiveTheme(AppTheme theme) =>
        theme == AppTheme.System ? DetectSystemTheme() : theme;

    /// <summary>
    /// Đọc theme Windows hiện tại từ Registry.
    /// AppsUseLightTheme: 1 = Light, 0 = Dark.
    /// </summary>
    private static AppTheme DetectSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                return value == 0 ? AppTheme.Dark : AppTheme.Light;
            }
        }
        catch
        {
            // Không đọc được Registry → fallback Light, không crash app.
        }

        return AppTheme.Light;
    }

    /// <summary>
    /// Thay thế dictionary theme (Light/Dark) trong MergedDictionaries của App.
    /// Giữ nguyên ThemeResources.xaml (spacing, font).
    /// </summary>
    private static void SwapThemeDictionary(AppTheme effective)
    {
        var app = Application.Current;
        if (app is null)
        {
            // Unit test không có Application.Current → no-op.
            return;
        }

        var newUri = new Uri(
            effective == AppTheme.Dark ? DarkPackUri : LightPackUri,
            UriKind.Absolute);

        var merged = app.Resources.MergedDictionaries;

        // Xóa theme dictionary cũ (LightTheme.xaml hoặc DarkTheme.xaml)
        for (int i = merged.Count - 1; i >= 0; i--)
        {
            var src = merged[i].Source?.OriginalString;
            if (src is null)
            {
                continue;
            }

            if (src.Contains("LightTheme.xaml", StringComparison.Ordinal) ||
                src.Contains("DarkTheme.xaml", StringComparison.Ordinal))
            {
                merged.RemoveAt(i);
            }
        }

        merged.Add(new ResourceDictionary { Source = newUri });
    }
}