using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed class LogsViewModel : PageViewModelBase
{
    public override NavigationItemKey Key => NavigationItemKey.Logs;

    public LogsViewModel()
    {
        Title = "Logs";
        Description = "Log console & change history — sẽ hiển thị ở Phase 21+.";
    }
}