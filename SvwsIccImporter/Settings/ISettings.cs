namespace SvwsIccImporter.Settings
{
    public interface ISettings
    {
        public IIccSettings Icc { get; }

        public ISvwsSettings Svws { get; }
    }
}
