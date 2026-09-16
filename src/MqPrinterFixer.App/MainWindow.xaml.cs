using System.Windows;
using MqPrinterFixer.App.ViewModels;

namespace MqPrinterFixer.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}