using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed class Error0x11BViewModel : PageViewModelBase
{
    public override NavigationItemKey Key => NavigationItemKey.Error0x11B;

    public Error0x11BViewModel()
    {
        Title = "Error 0x0000011B";
        Description = "Diagnose 0x11B — sẽ hiển thị ở Phase 17.";
    }
}