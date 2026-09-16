using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed class SettingsViewModel : PageViewModelBase
{
    public override NavigationItemKey Key => NavigationItemKey.Settings;

    public SettingsViewModel()
    {
        Title = "Settings";
        Description = "Appearance, language, role detection — sẽ hiển thị ở Phase 23.";
    }
}