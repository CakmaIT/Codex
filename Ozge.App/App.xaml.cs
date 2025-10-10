using System;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.Sources.Clear();
                builder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(prefix: "OZGE_");
            })
            .UseSerilog()
            .ConfigureServices((_, services) => RegisterServices(services))
            .Build();

        await _host.StartAsync();

        var initializer = _host.Services.GetRequiredService<AppInitializationHostedService>();
        await initializer.EnsureInitializedAsync();

        var dependencyChecker = _host.Services.GetRequiredService<DependencyCheckService>();
        var dependencyResult = dependencyChecker.Validate();
        if (!dependencyResult.Success)
        {
            var message = dependencyResult.ToString();
            Log.Logger.Error("Startup dependency validation failed: {Message}", message);
            MessageBox.Show(
                message,
                "Eksik Bileşenler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        try
        {
            var teacherWindow = _host.Services.GetRequiredService<TeacherDashboardWindow>();
            MainWindow = teacherWindow;
            teacherWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "TeacherDashboardWindow oluşturulamadı");
            MessageBox.Show(
                ex.ToString(),
                "Uygulama Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (_, args) =>
        {
            Log.Logger.Error(args.Exception, "Beklenmeyen hata");
            MessageBox.Show(
                args.Exception.Message,
                "Beklenmeyen Hata",
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
        services.AddSingleton<ISoundSettingsService, SoundSettingsService>();
        services.AddSingleton<ISoundEffectPlayer, SoundEffectPlayer>();
        services.AddSingleton<IQuizSessionService, QuizSessionService>();
        services.AddSingleton<IQuestionImportService, QuestionImportService>();
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
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}


