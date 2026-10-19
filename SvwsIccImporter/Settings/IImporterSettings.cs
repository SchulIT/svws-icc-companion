using System.Collections.Generic;

namespace SvwsIccImporter.Settings;

public interface IImporterSettings
{
    public IList<string> DisabledImporters { get; set; }
}