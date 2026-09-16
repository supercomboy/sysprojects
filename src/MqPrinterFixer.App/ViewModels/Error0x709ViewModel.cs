using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed class Error0x709ViewModel : PageViewModelBase
{
    public override NavigationItemKey Key => NavigationItemKey.Error0x709;

    public Error0x709ViewModel()
    {
        Title = "Error 0x00000709";
        Description = "Diagnose 0x709 — sẽ hiển thị ở Phase 18.";
    }
}