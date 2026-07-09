using System;

namespace SvwsIccImporter.Settings
{
    public delegate void SettingsSavedEventHandler(ISettingsManager manager, SettingsSavedEventArgs args);

    public class SettingsSavedEventArgs : EventArgs
    {
        public SettingsSavedEventArgs()
        {

        }
    }
}
