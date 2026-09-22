using Avalonia.Controls.ApplicationLifetimes;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using SchulIT.IccImport;
using SvwsClient.Tool.Unterricht;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.UI;
using SvwsIccImporter.UI.Dialog;
using SvwsIccImporter.View;
using SvwsIccImporter.ViewModel;
using SvwsIccImporter.ViewModel.Import;

namespace SvwsIccImporter.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddSingleton(typeof(IFANavigationPageFactory), sp => new NavigationPageFactory(sp));
            collection.AddSingleton(typeof(IClassicDesktopStyleApplicationLifetime), sp => Avalonia.Application.Current.ApplicationLifetime);

            collection.AddSingleton<ISettingsManager, JsonSettingsManager>();
            collection.AddSingleton<IWindowManager, WindowManager>();
            collection.AddSingleton<IDialogHelper, DialogHelper>();
            collection.AddSingleton<IDispatcherHelper, DispatcherHelper>(); // TODO
            collection.AddSingleton<IRestClientFactory, RestClientFactory>();
            collection.AddSingleton<IHttpClientFactory, HttpClientFactory>();
            collection.AddSingleton<IIccImporterConfigurator, IccImporterConfigurator>();
            collection.AddSingleton<IIccImporter, IccImporter>();
            collection.AddSingleton<ICachedUnterrichtResolver, CachedUnterrichtResolver>();
            collection.AddSingleton<IIdResolver, IdResolver>();
            collection.AddSingleton<INameResolver, NameResolver>();
            collection.AddSingleton<UnterrichtResolver>();

            collection.AddSingleton<SettingsViewModel>();
            collection.AddSingleton<SplashScreenViewModel>();
            collection.AddSingleton<SvwsViewModel>();
            collection.AddSingleton<AboutViewModel>();
            collection.AddSingleton<ImportViewModel>();

            collection.AddSingleton<IImportTaskViewModel, ImportGradesTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportGradeTeachersTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportGradeMembershipsTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportLearningManagementSystemsTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportPrivacyCategoriesTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportStudentLearningManagementSystemsTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportStudentsTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportStudyGroupMembershipTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportStudyGroupsTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportSubjectsTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportTeachersTaskViewModel>();
            collection.AddSingleton<IImportTaskViewModel, ImportTuitionsTaskViewModel>();

            collection.AddSingleton<ImportPage>();
            collection.AddSingleton<AboutPage>();
            collection.AddSingleton<SettingsPage>();
            collection.AddSingleton<SplashScreen>();
            collection.AddSingleton<MainView>();
        }
    }
}
