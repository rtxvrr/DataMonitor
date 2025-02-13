using System.Windows;
using DataMonitor.Data.Context;
using DataMonitor.Services;
using DataMonitor.ViewModels;
using DataMonitor.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataMonitor
{
    public partial class App : Application
    {
        private IHost _host;

        public IHost Host => _host;

        public App()
        {
            _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // context
                    services.AddDbContextFactory<DataMonitorContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DataMonitorConnection")));
                    // services
                    services.AddTransient<ILoggerService, LoggerService>();
                    services.AddTransient<IWindowService, WindowService>();
                    services.AddTransient<IFileReaderService, FileReaderService>();
                    services.AddTransient<IFileDialogService, FileDialogService>();
                    // views vm`s
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<AddRecordViewModel>();
                    services.AddTransient<AddRecordWindow>();

                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
