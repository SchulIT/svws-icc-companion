using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SvwsIccImporter.ViewModel.Form
{
    public partial class SettingsForm : ObservableValidator
    {
        [Required]
        [ObservableProperty]
        private string? iccEndpoint = string.Empty;

        [Required]
        [ObservableProperty]
        private string? iccToken = string.Empty;

        [Required]
        [ObservableProperty]
        private string? svwsServer = string.Empty;

        [Required]
        [ObservableProperty]
        private string? svwsSchema = string.Empty;

        [Required]
        [ObservableProperty]
        private string? svwsUsername = string.Empty;

        [ObservableProperty]
        private string? svwsPassword = string.Empty;

        [ObservableProperty]
        private bool onlyVisible = true;

        [ObservableProperty]
        private bool ignoreCertificateWarnings = false;
    }
}
