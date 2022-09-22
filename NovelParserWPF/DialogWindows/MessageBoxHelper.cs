using System.Windows;

namespace NovelParserWPF.DialogWindows
{
    static class MessageBoxHelper
    {
        public static void ShowErrorWindow(string message)
        {
            _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
