using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MqPrinterFixer.App.ViewModels;
using MqPrinterFixer.App.Views;

namespace MqPrinterFixer.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnRenameComputerClick(object sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app)
        {
            return;
        }

        var dialogVm = app.Services.GetRequiredService<RenameComputerDialogViewModel>();
        var dialog = new RenameComputerDialog(dialogVm)
        {
            Owner = this
        };

        var result = dialog.ShowDialog();

        // Nếu rename thành công (hoặc user bấm Restart Later), refresh header.
        if (result == true && DataContext is MainViewModel main)
        {
            _ = main.LoadSystemInfoAsync();
        }
    }
}