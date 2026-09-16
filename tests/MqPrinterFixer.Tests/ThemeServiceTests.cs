using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.Services;
using Xunit;

namespace MqPrinterFixer.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void ApplyTheme_Light_SetsCurrentAndEffective()
    {
        var svc = new ThemeService();
        svc.ApplyTheme(AppTheme.Light);
        Assert.Equal(AppTheme.Light, svc.CurrentTheme);
        Assert.Equal(AppTheme.Light, svc.EffectiveTheme);
    }

    [Fact]
    public void ApplyTheme_Dark_SetsCurrentAndEffective()
    {
        var svc = new ThemeService();
        svc.ApplyTheme(AppTheme.Dark);
        Assert.Equal(AppTheme.Dark, svc.CurrentTheme);
        Assert.Equal(AppTheme.Dark, svc.EffectiveTheme);
    }

    [Fact]
    public void ApplyTheme_System_ResolvesToLightOrDark()
    {
        var svc = new ThemeService();
        svc.ApplyTheme(AppTheme.System);

        Assert.Equal(AppTheme.System, svc.CurrentTheme);
        Assert.True(
            svc.EffectiveTheme == AppTheme.Light || svc.EffectiveTheme == AppTheme.Dark,
            $"EffectiveTheme phải là Light hoặc Dark, thực tế: {svc.EffectiveTheme}");
    }

    [Fact]
    public void ApplyTheme_RaisesThemeChangedEvent()
    {
        var svc = new ThemeService();
        var raised = 0;
        svc.ThemeChanged += (_, _) => raised++;

        svc.ApplyTheme(AppTheme.Dark);
        svc.ApplyTheme(AppTheme.Light);
        svc.ApplyTheme(AppTheme.System);

        Assert.Equal(3, raised);
    }

    [Fact]
    public void Default_CurrentTheme_IsSystem()
    {
        var svc = new ThemeService();
        Assert.Equal(AppTheme.System, svc.CurrentTheme);
    }
}