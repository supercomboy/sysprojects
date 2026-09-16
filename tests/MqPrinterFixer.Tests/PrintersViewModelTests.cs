using MqPrinterFixer.App.Models;
using MqPrinterFixer.App.ViewModels;
using MqPrinterFixer.Tests.Fakes;
using Xunit;

namespace MqPrinterFixer.Tests;

public class PrintersViewModelTests
{
    [Fact]
    public async Task LoadAsync_PopulatesPrintersList()
    {
        var fake = new FakePrinterService();
        fake.PrintersToReturn.Add(FakePrinterService.Build("Canon LBP2900"));
        fake.PrintersToReturn.Add(FakePrinterService.Build("HP LaserJet"));

        var vm = new PrintersViewModel(fake);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Printers.Count);
    }

    [Fact]
    public async Task LoadAsync_SelectsDefaultPrinter()
    {
        var fake = new FakePrinterService();
        fake.PrintersToReturn.Add(FakePrinterService.Build("Printer A", isDefault: false));
        fake.PrintersToReturn.Add(FakePrinterService.Build("Printer B", isDefault: true));
        fake.PrintersToReturn.Add(FakePrinterService.Build("Printer C", isDefault: false));

        var vm = new PrintersViewModel(fake);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.NotNull(vm.SelectedPrinter);
        Assert.Equal("Printer B", vm.SelectedPrinter!.Name);
    }

    [Fact]
    public async Task LoadAsync_EmptyList_SetsStatusMessage()
    {
        var fake = new FakePrinterService();

        var vm = new PrintersViewModel(fake);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Empty(vm.Printers);
        Assert.Contains("Không", vm.StatusMessage);
    }

    [Fact]
    public async Task LoadAsync_NonEmptyList_ReportsCount()
    {
        var fake = new FakePrinterService();
        fake.PrintersToReturn.Add(FakePrinterService.Build("A"));
        fake.PrintersToReturn.Add(FakePrinterService.Build("B"));

        var vm = new PrintersViewModel(fake);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Contains("2", vm.StatusMessage);
    }

    [Fact]
    public void Constructor_SetsTitleAndDescription()
    {
        var fake = new FakePrinterService();
        var vm = new PrintersViewModel(fake);

        Assert.Equal("Printers", vm.Title);
        Assert.Contains("inventory", vm.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_OrdersDefaultFirst()
    {
        var fake = new FakePrinterService();
        fake.PrintersToReturn.Add(FakePrinterService.Build("Alpha", isDefault: false));
        fake.PrintersToReturn.Add(FakePrinterService.Build("Beta", isDefault: true));
        fake.PrintersToReturn.Add(FakePrinterService.Build("Gamma", isDefault: false));

        var vm = new PrintersViewModel(fake);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal("Beta", vm.Printers[0].Name);
    }
}