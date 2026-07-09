using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchulIT.SvwsClient;
using SvwsIccImporter.Service;
using SvwsIccImporter.Settings;
using SvwsIccImporter.ViewModel.Import;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SvwsIccImporter.ViewModel
{
    public partial class ImportViewModel : ObservableRecipient
    {
        #region Properties

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
        private bool isBusy = false;

        [ObservableProperty]
        private string busyText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
        public Schuljahresabschnitt? selectedSection = null;

        public ObservableCollection<Schuljahresabschnitt> Sections { get; } = new ObservableCollection<Schuljahresabschnitt>();

        public ImportGradeMembershipsTaskViewModel GradeMembershipsViewModel { get { return importViewModels.Where(x => x is ImportGradeMembershipsTaskViewModel).Cast<ImportGradeMembershipsTaskViewModel>().First(); } }
        public ImportGradesTaskViewModel GradesViewModel { get { return importViewModels.Where(x => x is ImportGradesTaskViewModel).Cast<ImportGradesTaskViewModel>().First(); } }
        public ImportGradeTeachersTaskViewModel GradeTeachersViewModel { get { return importViewModels.Where(x => x is ImportGradeTeachersTaskViewModel).Cast<ImportGradeTeachersTaskViewModel>().First(); } }
        public ImportLearningManagementSystemsTaskViewModel LmsViewModel { get { return importViewModels.Where(x => x is ImportLearningManagementSystemsTaskViewModel).Cast<ImportLearningManagementSystemsTaskViewModel>().First(); } }
        public ImportPrivacyCategoriesTaskViewModel PrivacyCategoriesViewModel { get { return importViewModels.Where(x => x is ImportPrivacyCategoriesTaskViewModel).Cast<ImportPrivacyCategoriesTaskViewModel>().First(); } }
        public ImportStudentLearningManagementSystemsTaskViewModel StudentsLmsViewModel { get { return importViewModels.Where(x => x is ImportStudentLearningManagementSystemsTaskViewModel).Cast<ImportStudentLearningManagementSystemsTaskViewModel>().First(); } }
        public ImportStudentsTaskViewModel StudentsViewModel { get { return importViewModels.Where(x => x is ImportStudentsTaskViewModel).Cast<ImportStudentsTaskViewModel>().First(); } }
        public ImportStudyGroupMembershipTaskViewModel StudyGroupMembershipsViewModel { get { return importViewModels.Where(x => x is ImportStudyGroupMembershipTaskViewModel).Cast<ImportStudyGroupMembershipTaskViewModel>().First(); } }
        public ImportStudyGroupsTaskViewModel StudyGroupsViewModel { get { return importViewModels.Where(x => x is ImportStudyGroupsTaskViewModel).Cast<ImportStudyGroupsTaskViewModel>().First(); } }
        public ImportSubjectsTaskViewModel SubjectsViewModel { get { return importViewModels.Where(x => x is ImportSubjectsTaskViewModel).Cast<ImportSubjectsTaskViewModel>().First(); } }
        public ImportTeachersTaskViewModel TeachersViewModel { get { return importViewModels.Where(x => x is ImportTeachersTaskViewModel).Cast<ImportTeachersTaskViewModel>().First(); } }
        public ImportTuitionsTaskViewModel TuitionsViewModel { get { return importViewModels.Where(x => x is ImportTuitionsTaskViewModel).Cast<ImportTuitionsTaskViewModel>().First(); } }


        #endregion

        private CancellationTokenSource? cts;

        #region Commands

        public AsyncRelayCommand LoadSectionsCommand { get; private set; }
        public AsyncRelayCommand ImportCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        #endregion

        #region Services

        private readonly IRestClientFactory restClientFactory;
        private readonly ISettingsManager settingsManager;
        private readonly IIccImporterConfigurator importConfigurator;
        private readonly IEnumerable<IImportTaskViewModel> importViewModels;

        #endregion

        public ImportViewModel(IRestClientFactory restClientFactory, ISettingsManager settingsManager, IIccImporterConfigurator importConfigurator, IEnumerable<IImportTaskViewModel> importViewModels)
        {
            this.restClientFactory = restClientFactory;
            this.settingsManager = settingsManager;
            this.importConfigurator = importConfigurator;
            this.importViewModels = importViewModels;

            ObserveImportModels();

            LoadSectionsCommand = new AsyncRelayCommand(LoadSectionsAsync);
            ImportCommand = new AsyncRelayCommand(Import, CanImport);
            CancelCommand = new RelayCommand(Cancel, CanCancel);
        }

        private void ObserveImportModels()
        {
            foreach(var viewModel in importViewModels)
            {
                viewModel.PropertyChanged += ImportViewModelPropertyChanged;
            }
        }

        private void ImportViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportTaskViewModelBase.IsEnabled))
            {
                ImportCommand.NotifyCanExecuteChanged();
            }
        }

        public async Task LoadSectionsAsync()
        {
            try
            {
                var client = restClientFactory.GetClient();
                var stammdaten = await client.GetSchuleStammdatenAsync(settingsManager.Settings.Svws.Schema);

                Sections.Clear();
                
                foreach(var section in stammdaten.Abschnitte)
                {
                    Sections.Add(section);
                }

                SelectedSection = Sections.FirstOrDefault(x => x.Id == stammdaten.IdSchuljahresabschnitt);
            }
            catch (Exception ex)
            {
                // TODO
            }
        }

        private async Task Import()
        {
            if(SelectedSection == null)
            {
                return;
            }

            try
            {
                cts = new CancellationTokenSource();

                importConfigurator.Configure();

                var sortedTasks = importViewModels
                    .Where(x => x.GetType().GetCustomAttribute<Priority>() != null)
                    .OrderBy(x => x.GetType().GetCustomAttribute<Priority>().Value)
                    .ToList();

                foreach (var viewModel in sortedTasks)
                {
                    if (!viewModel.IsEnabled)
                    {
                        viewModel.CurrentStatus = ViewModel.Import.TaskStatus.None;
                    }
                    else
                    {
                        viewModel.CurrentStatus = ViewModel.Import.TaskStatus.Pending;
                    }
                }

                foreach (var viewModel in sortedTasks.Where(x => x.IsEnabled))
                {
                    if(cts.IsCancellationRequested)
                    {
                        return;
                    }

                    var isSuccessful = await viewModel.RunAsync(SelectedSection);

                    if (!isSuccessful)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO
            }
            finally
            {
                IsBusy = false;
                cts = null;
            }
        }

        private bool CanImport() => !IsBusy && SelectedSection != null && importViewModels.Any(x => x.IsEnabled);

        private void Cancel()
        {
            cts?.Cancel();
        }

        private bool CanCancel() => IsBusy;
    }
}
