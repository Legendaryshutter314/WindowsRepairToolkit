namespace WindowsRepairToolkit.Services;

public interface IDialogService
{
    bool Confirm(string message, string title, bool warning = false);
    void ShowInfo(string message, string title = "Windows Repair Toolkit");
    void ShowWarning(string message, string title = "Windows Repair Toolkit");
    void ShowError(string message, string title = "Windows Repair Toolkit");
}
