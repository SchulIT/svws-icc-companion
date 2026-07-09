using Newtonsoft.Json;

namespace SvwsIccImporter.Settings
{
    public class JsonSettings : ISettings
    {
        [JsonProperty("icc")]
        public IIccSettings Icc { get; } = new JsonIccSettings();

        [JsonProperty("svws")]
        public ISvwsSettings Svws { get; } = new JsonSvwsSettings();
    }
}
