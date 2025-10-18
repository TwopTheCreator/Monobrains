using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Monobrains.Core;
using Monobrains.UI;
using Monobrains.Services;
using Monobrains.Plugins;

namespace Monobrains
{
    public partial class App : Application
    {
        private IHost _host;
        private MainWindow _mainWindow;

        protected override async void OnStartup(StartupEventArgs e)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<PythonBackendService>();
                    services.AddSingleton<CSharpBackupService>();
                    services.AddSingleton<IPythonBackendService, BackendServiceManager>();
                    services.AddSingleton<ICodeAnalysisService, CodeAnalysisService>();
                    services.AddSingleton<IDebuggingService, DebuggingService>();
                    services.AddSingleton<IExecutionService, ExecutionService>();
                    services.AddSingleton<IPluginManager, PluginManager>();
                    services.AddSingleton<ISyntaxHighlightingService, SyntaxHighlightingService>();
                    services.AddSingleton<IFileManagerService, FileManagerService>();
                    services.AddSingleton<IProjectManagerService, ProjectManagerService>();
                    services.AddSingleton<ICodeCompletionService, CodeCompletionService>();
                    services.AddSingleton<IRefactoringService, RefactoringService>();
                    services.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                });

            _host = builder.Build();

            await _host.StartAsync();

            var serviceProvider = _host.Services;
            _mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            _mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
