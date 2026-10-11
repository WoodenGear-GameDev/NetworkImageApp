using Avalonia.Controls;
using ServerApp.ViewModels;

namespace ServerApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}