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

    private void ConsoleTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.CaretIndex = textBox.Text?.Length ?? 0;
        }
    }
}