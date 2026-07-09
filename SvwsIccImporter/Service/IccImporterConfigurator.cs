using SchulIT.IccImport;
using SvwsIccImporter.Settings;

namespace SvwsIccImporter.Service
{
    public class IccImporterConfigurator : IIccImporterConfigurator
    {

        private readonly ISettingsManager settingsManager;
        private readonly IIccImporter iccImporter;

        public IccImporterConfigurator(ISettingsManager settingsManager, IIccImporter iccImporter)
        {
            this.settingsManager = settingsManager;
            this.iccImporter = iccImporter;
        }

        public void Configure()
        {
            iccImporter.BaseUrl = settingsManager.Settings.Icc.Endpoint;
            iccImporter.Token = settingsManager.Settings.Icc.Token;
        }
    }
}
