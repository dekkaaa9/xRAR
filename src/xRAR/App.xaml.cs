using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using xRAR.Services;

namespace xRAR
{
    public partial class App : Application
    {
        public static string? StartupFilePath { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Capture file path from command-line args (double-click from Explorer)
            if (e.Args.Length > 0 && !string.IsNullOrEmpty(e.Args[0]))
            {
                string arg = e.Args[0].Trim('"');
                if (File.Exists(arg))
                    StartupFilePath = arg;
            }

            // Auto-register file associations on startup (silent, user-level, no admin)
            try
            {
                FileAssociationService.AutoRegisterIfNeeded();
            }
            catch { }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"xRAR encountered an error:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "xRAR Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"Fatal error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "xRAR Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
