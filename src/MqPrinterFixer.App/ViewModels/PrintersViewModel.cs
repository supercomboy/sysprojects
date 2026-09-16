using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.ViewModels;

public sealed partial class PrintersViewModel : PageViewModelBase
{
    private readonly IPrinterService _printerService;

    public override NavigationItemKey Key => NavigationItemKey.Printers;

    public ObservableCollection<PrinterInfo> Printers { get; } = new();

    [ObservableProperty]
    private PrinterInfo? _selectedPrinter;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public PrintersViewModel(IPrinterService printerService)
    {
        _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));

        Title = "Printers";
        Description = "Printer inventory — tất cả máy in đã cài trên máy.";

        _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = "Scanning printers...";

        try
        {
            var list = await _printerService.GetAllPrintersAsync(cancellationToken);

            Printers.Clear();
            foreach (var p in list.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                Printers.Add(p);
            }

            StatusMessage = Printers.Count == 0
                ? "Không tìm thấy máy in nào."
                : $"{Printers.Count} máy in.";

            SelectedPrinter = Printers.FirstOrDefault(p => p.IsDefault) ?? Printers.FirstOrDefault();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public static string GetConnectionTypeDisplay(PrinterConnectionType type) =>
        PrinterStatusMapper.ToDisplayName(type);

    public static string GetStatusDisplay(PrinterStatus status) =>
        PrinterStatusMapper.ToDisplayName(status);
}