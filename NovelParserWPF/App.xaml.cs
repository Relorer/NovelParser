// ReSharper disable RedundantExtendsListEntry

using System.Windows;
using NovelParserWPF.ViewModels;

namespace NovelParserWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly MainWindow _mainWindow;

    public App()
    {
        InitializeComponent();

        _mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel()
        };
    }
    private void OnStartup(object sender, StartupEventArgs e)
    {
       _mainWindow.Show();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
            
    }
}