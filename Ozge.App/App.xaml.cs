using System.Configuration;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ozge.App.Services;
using Ozge.App.State;
using Ozge.App.ViewModels;
using Ozge.App.Views;
using Ozge.Core.Services;
using Ozge.Data.Extensions;
using Ozge.Data.Seed;
using Ozge.Ocr.Services;

namespace Ozge.App;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ozge2");
        Directory.CreateDirectory(appData);
        Directory.CreateDirectory(Path.Combine(appData, "Logs"));
        Directory.CreateDirectory(Path.Combine(appData, "Snapshots"));
        Directory.CreateDirectory(Path.Combine(appData, "Backups"));

        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            })
            .ConfigureServices((context, services) =>
            {
                var databasePath = Path.Combine(appData, "ozge2.db");
                services.AddSingleton<AppPaths>(_ => new AppPaths(appData));
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                services.AddOzgeData(databasePath);
                services.AddOzgeOcr();
                services.AddSingleton<StateStore>();
                services.AddSingleton<WindowCoordinator>();
                services.AddSingleton<TeacherDashboardViewModel>();
                services.AddSingleton<ProjectorViewModel>();
                services.AddTransient<TeacherDashboardWindow>();
                services.AddTransient<ProjectorWindow>();
            })
            .Build();

        _host.Start();

        using var scope = _host.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        var dataStore = scope.ServiceProvider.GetRequiredService<IDataStore>();
        dataStore.InitializeAsync().GetAwaiter().GetResult();
        seeder.SeedAsync().GetAwaiter().GetResult();

        var window = scope.ServiceProvider.GetRequiredService<TeacherDashboardWindow>();
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
