using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SvwsIccImporter.DependencyInjection;
using SvwsIccImporter.View;

namespace SvwsIccImporter;

public partial class App : Application
{
    private ServiceProvider services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddCommonServices();

        services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splashScreen = services.GetRequiredService<SplashScreen>();
            desktop.MainWindow = splashScreen;

            splashScreen.Show();
            await splashScreen.ViewModel.Initialize();

            var mainView = services.GetRequiredService<MainView>();
            desktop.MainWindow = mainView;
            mainView.Show();

            splashScreen.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }
}