<<<<<<< Updated upstream
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
=======
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ozge.App.Infrastructure;
using Ozge.App.Presentation;
using Ozge.App.Presentation.ViewModels;
using Ozge.App.Presentation.Windows;
using Ozge.Core;
using Ozge.Core.Contracts;
using Ozge.Data.Extensions;
using Ozge.Data.Services;
using Ozge.Ocr.Extensions;
using Serilog;
using MessageBox = System.Windows.MessageBox;

namespace Ozge.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ConfigureLogging();

        _host = Host.CreateDefaultBuilder(e.Args)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                configurationBuilder.Sources.Clear();
                configurationBuilder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(prefix: "OZGE_");
            })
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                RegisterServices(services);
            })
            .Build();

        await _host.StartAsync();

        var initializer = _host.Services.GetRequiredService<AppInitializationHostedService>();
        Log.Logger.Information("Starting application initialisation...");
        await initializer.EnsureInitializedAsync();
        Log.Logger.Information("Application initialisation completed.");

        var dependencyChecker = _host.Services.GetRequiredService<DependencyCheckService>();
        var dependencyResult = dependencyChecker.Validate();
        if (!dependencyResult.Success)
        {
            var message = dependencyResult.ToString();
            Log.Logger.Error("Startup dependency validation failed: {Message}", message);
            MessageBox.Show(
                message,
                "Missing Components",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        try
        {
            var teacherWindow = _host.Services.GetRequiredService<TeacherDashboardWindow>();
            teacherWindow.Show();
            Log.Logger.Information("Main window set: {Title}", System.Windows.Application.Current.MainWindow?.Title ?? "<null>");
            Log.Logger.Information("Teacher dashboard window displayed.");
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Failed to create or show the TeacherDashboardWindow");
            MessageBox.Show(
                ex.ToString(),
                "Application Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (_, args) =>
        {
            Log.Logger.Error(args.Exception, "Unhandled dispatcher exception");
            MessageBox.Show(
                args.Exception.Message,
                "Unexpected Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
    }

    private static void ConfigureLogging()
    {
        var logsFolder = Path.Combine(AppConstants.GetLocalAppDataRoot(), AppConstants.LogsFolderName);
        Directory.CreateDirectory(logsFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(logsFolder, "ozge2-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateLogger();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileSystem, LocalFileSystem>();
        services.AddSingleton<IAssetProvisioner, AssetProvisioner>();
        services.AddSingleton<IDisplayService, DisplayService>();
        services.AddSingleton<IAppStateStore, AppStateStore>();
        services.AddSingleton<IProjectorWindowManager, ProjectorWindowManager>();
        services.AddSingleton<IJobQueue, NullJobQueue>();
        services.AddSingleton<IQuizSessionService, QuizSessionService>();
        services.AddSingleton<DependencyCheckService>();

        services.AddSingleton<TeacherDashboardWindow>();
        services.AddSingleton<ProjectorWindow>();

        services.AddTransient<TeacherDashboardViewModel>();
        services.AddTransient<ProjectorViewModel>();

        var databasePath = Path.Combine(AppConstants.GetLocalAppDataRoot(), AppConstants.DatabaseFileName);
        services.AddOzgeData(databasePath);
        services.AddOzgeOcr();

        services.AddSingleton<AppInitializationHostedService>();
        services.AddHostedService<BackgroundJobHostService>();
>>>>>>> Stashed changes
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
<<<<<<< Updated upstream
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
=======
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}


>>>>>>> Stashed changes
