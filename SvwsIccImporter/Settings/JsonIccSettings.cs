using Newtonsoft.Json;

namespace SvwsIccImporter.Settings
{
    public class JsonIccSettings : IIccSettings
    {
        [JsonProperty("endpoint")]
        public string? Endpoint { get; set; } = null;

        [JsonProperty("token")]
        public string? Token { get; set; } = null;
    }
}
