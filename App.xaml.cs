using System.Windows;
using System.Windows.Threading;
using WindowsRepairToolkit.Services;
using WindowsRepairToolkit.ViewModels;

namespace WindowsRepairToolkit;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        RegisterGlobalExceptionHandlers();
        StartupDiagnostics.Write("Application startup entered.");

        try
        {
            base.OnStartup(e);
            StartupDiagnostics.Write("WPF base startup completed.");

            // Construct the view first. If XAML loading fails, the exception is now
            // surfaced to the user and written to the startup log instead of the
            // WinExe process silently terminating.
            var window = new MainWindow();
            MainWindow = window;
            StartupDiagnostics.Write("MainWindow XAML initialized.");

            var mainViewModel = BuildMainViewModel();
            StartupDiagnostics.Write("MVVM composition completed.");

            window.DataContext = mainViewModel;
            window.Show();
            window.Activate();
            window.Focus();
            StartupDiagnostics.Write("MainWindow shown successfully.");
        }
        catch (Exception ex)
        {
            HandleFatalStartupException(ex);
        }
    }

    private static MainWindowViewModel BuildMainViewModel()
    {
        var dialogs = new WpfDialogService();
        var dispatcher = new WpfUiDispatcher();
        var logService = new LogService();
        var operation = new OperationViewModel(logService, dialogs, dispatcher);

        return new MainWindowViewModel(
            operation,
            dialogs,
            new SystemRepairViewModel(operation, dialogs),
            new CleanupStorageViewModel(operation),
            new StartupViewModel(operation, dialogs),
            new DebloatViewModel(operation, dialogs),
            new ProfileViewModel(operation),
            new SearchExplorerViewModel(operation),
            new WindowsUpdateViewModel(operation, dialogs));
    }

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += Application_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StartupDiagnostics.WriteException("Unhandled WPF dispatcher exception", e.Exception);

        MessageBox.Show(
            $"Windows Repair Toolkit encountered an unexpected error.\n\n{e.Exception.Message}\n\nDiagnostic log:\n{StartupDiagnostics.LogPath}",
            "Windows Repair Toolkit",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Do not keep running after an unknown UI exception in an elevated repair tool.
        e.Handled = true;
        Shutdown(-1);
    }

    private static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            StartupDiagnostics.WriteException("Unhandled AppDomain exception", ex);
        else
            StartupDiagnostics.Write($"Unhandled AppDomain exception object: {e.ExceptionObject}");
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        StartupDiagnostics.WriteException("Unobserved task exception", e.Exception);
        e.SetObserved();
    }

    private void HandleFatalStartupException(Exception ex)
    {
        StartupDiagnostics.WriteException("Fatal startup exception", ex);

        try
        {
            MessageBox.Show(
                $"Windows Repair Toolkit could not finish starting.\n\n{ex.Message}\n\nA diagnostic log was written to:\n{StartupDiagnostics.LogPath}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            Shutdown(-1);
        }
    }
}
