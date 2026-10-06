using System.Windows;

namespace WindowsRepairToolkit.Services;

public sealed class WpfDialogService : IDialogService
{
    public bool Confirm(string message, string title, bool warning = false)
        => MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            warning ? MessageBoxImage.Warning : MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void ShowInfo(string message, string title = "Windows Repair Toolkit")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowWarning(string message, string title = "Windows Repair Toolkit")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowError(string message, string title = "Windows Repair Toolkit")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}
