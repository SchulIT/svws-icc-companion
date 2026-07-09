using System.Threading.Tasks;

namespace SvwsIccImporter.Settings
{
    public interface ISettingsManager
    {
        ISettings Settings { get; }

        event SettingsSavedEventHandler SettingsSaved;

        Task LoadSettingsAsync();

        Task SaveSettingsAsync();
    }
}
