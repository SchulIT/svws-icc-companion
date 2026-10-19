using System.Collections.Generic;
using Newtonsoft.Json;

namespace SvwsIccImporter.Settings;

public class JsonImporterSettings : IImporterSettings
{
    [JsonProperty("disabled_importers")]
    public IList<string> DisabledImporters { get; set; } = new List<string>();
}