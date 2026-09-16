using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed class NetworkSharingViewModel : PageViewModelBase
{
    public override NavigationItemKey Key => NavigationItemKey.NetworkSharing;

    public NetworkSharingViewModel()
    {
        Title = "Network & Sharing";
        Description = "Network discovery, sharing — sẽ hiển thị ở Phase 12+.";
    }
}