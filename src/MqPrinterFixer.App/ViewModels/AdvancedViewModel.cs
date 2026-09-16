using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed class AdvancedViewModel : PageViewModelBase
{
    public override NavigationItemKey Key => NavigationItemKey.Advanced;

    public AdvancedViewModel()
    {
        Title = "Advanced";
        Description = "Spooler, ports, policies, firewall — sẽ hiển thị ở Phase 19+.";
    }
}