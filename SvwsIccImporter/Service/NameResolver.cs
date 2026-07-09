using SvwsClient.Tool.Unterricht;
using System.Linq;

namespace SvwsIccImporter.Service
{
    public class NameResolver : INameResolver
    {
        public string ResolveName(Unterricht unterricht)
        {
            if(unterricht.KursId == null) // Klassenunterricht
            {
                return $"{unterricht.Fach.Kuerzel}-{string.Join('-', unterricht.Klassen.Select(x => x.Name).Distinct().OrderBy(x => x))}";
            }

            return unterricht.KursBezeichnung;
        }
    }
}
