using System.Windows;
using MqPrinterFixer.App.ViewModels;

namespace MqPrinterFixer.App.Views;

public partial class RenameComputerDialog : Window
{
    public RenameComputerDialog(RenameComputerDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.CloseRequested += OnCloseRequested;
        Closed += (_, _) => viewModel.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested(object? sender, bool refreshParent)
    {
        DialogResult = refreshParent;
        Close();
    }
}