using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchulIT.IccImport;
using SvwsClient.Tool.Unterricht;
using SvwsIccImporter.DependencyInjection;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI;
using SvwsIccImporter.UI.Dialog;
using SvwsIccImporter.View;
using SvwsIccImporter.ViewModel;
using SvwsIccImporter.ViewModel.Import;
using System;
using System.IO;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using SplashScreen = SvwsIccImporter.View.SplashScreen;

namespace SvwsIccImporter
{
    public partial class App : Application
    {
        public static readonly IHost host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                var basePath =
                Path.GetDirectoryName(AppContext.BaseDirectory)
                ?? throw new DirectoryNotFoundException(
                    "Unable to find the base directory of the application."
                );
                _ = c.SetBasePath(basePath);
            })
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddHostedService<ApplicationHostService>();
                    services.AddSingleton<INavigationViewPageProvider, NavigationViewPageProvider>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    services.AddSingleton<INavigationService, NavigationService>();

                    services.AddSingleton<ISettingsManager, JsonSettingsManager>();
                    services.AddSingleton<IWindowManager, WindowManager>();
                    services.AddSingleton<IDialogHelper, DialogHelper>();
                    services.AddSingleton<IDispatcherHelper, DispatcherHelper>(); // TODO
                    services.AddSingleton<IRestClientFactory, RestClientFactory>();
                    services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
                    services.AddSingleton<IIccImporterConfigurator, IccImporterConfigurator>();
                    services.AddSingleton<IIccImporter, IccImporter>();
                    services.AddSingleton<ICachedUnterrichtResolver, CachedUnterrichtResolver>();
                    services.AddSingleton<IIdResolver, IdResolver>();
                    services.AddSingleton<INameResolver, NameResolver>();
                    services.AddSingleton<UnterrichtResolver>();

                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<SplashScreenViewModel>();
                    services.AddSingleton<SvwsViewModel>();
                    services.AddSingleton<AboutViewModel>();
                    services.AddSingleton<ImportViewModel>();

                    services.AddSingleton<IImportTaskViewModel, ImportGradesTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportGradeTeachersTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportGradeMembershipsTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportLearningManagementSystemsTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportPrivacyCategoriesTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportStudentLearningManagementSystemsTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportStudentsTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportStudyGroupMembershipTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportStudyGroupsTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportSubjectsTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportTeachersTaskViewModel>();
                    services.AddSingleton<IImportTaskViewModel, ImportTuitionsTaskViewModel>();

                    services.AddSingleton<ImportPage>();
                    services.AddSingleton<AboutPage>();
                    services.AddSingleton<SettingsPage>();
                    services.AddSingleton<INavigationWindow, MainView>();
                }
            )
            .Build();

        public static IServiceProvider Services
        {
            get { return host.Services; }
        }

        public App()
        {
            /*ApplicationAccentColorManager.Apply(
                Color.FromArgb(0xFF, 0x00, 0x63, 0xB1)
            );*/
        }

        private async void OnStart(object sender, StartupEventArgs e)
        {
            var splashViewModel = host.Services.GetService<SplashScreenViewModel>();
            var splash = new SplashScreen(splashViewModel);
            splash.Show();

            await splashViewModel.Initialize();
            await host.StartAsync();

            splash.Close();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            await host.StopAsync();
        }
    }
}
