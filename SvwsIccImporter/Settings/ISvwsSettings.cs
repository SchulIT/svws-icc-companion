namespace SvwsIccImporter.Settings
{
    public interface ISvwsSettings
    {
        public string? Server { get; set; }  

        public string? Schema { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool OnlyVisible { get; set; }

        bool IgnoreCertificateWarnings { get; set; }
    }
}
