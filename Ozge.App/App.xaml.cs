using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Ozge.App.Host;
using Ozge.App.Services;
using Ozge.App.ViewModels;
using Ozge.App.Views;
using Ozge.Core.Services;
using Ozge.Data.Extensions;
using Ozge.Data.Services;
using Ozge.Ocr.Services;

namespace Ozge.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var builder = Host.CreateDefaultBuilder(e.Args)
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
            })
            .UseSerilog((context, cfg) =>
            {
                var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ozge2", "Logs");
                Directory.CreateDirectory(logDirectory);
                cfg.MinimumLevel.Debug();
                cfg.WriteTo.File(Path.Combine(logDirectory, "ozge2-.log"), rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddOzgeSqlite(context.Configuration);
                services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
                services.AddSingleton<IAppStateStore, ImmutableAppStateStore>();
                services.AddSingleton<IScreenSelectionService, ScreenSelectionService>();
                services.AddSingleton<ProjectorActivationService>();
                services.AddSingleton<IContentParser, OfflineContentParser>();

                services.AddTransient<TeacherDashboardViewModel>();
                services.AddTransient<ProjectorViewModel>();
                services.AddTransient<TeacherDashboardWindow>();
                services.AddTransient<ProjectorWindow>();
            });

        _host = builder.Build();
        await _host.StartAsync();

        var initializer = _host.Services.GetRequiredService<IDatabaseInitializer>();
        await initializer.InitializeAsync();

        var screenService = _host.Services.GetRequiredService<IScreenSelectionService>();
        await screenService.InitializeAsync();

        var stateStore = _host.Services.GetRequiredService<IAppStateStore>();
        await stateStore.InitializeAsync();

        var mainWindow = _host.Services.GetRequiredService<TeacherDashboardWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<TeacherDashboardViewModel>();
        mainWindow.Show();

        var projector = _host.Services.GetRequiredService<ProjectorActivationService>();
        projector.TryShowProjectorWindow();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
