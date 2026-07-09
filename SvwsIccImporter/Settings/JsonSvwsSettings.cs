namespace SvwsIccImporter.Settings
{
    public class JsonSvwsSettings : ISvwsSettings
    {
        public string? Server { get; set; } = null;
        public string? Schema { get; set; } = null;
        public string? Username { get; set; } = null;
        public string? Password { get; set; } = null;

        public bool OnlyVisible { get; set; } = true;
        public bool IgnoreCertificateWarnings { get; set; } = false;
    }
}
