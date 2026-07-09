using SvwsClient.Tool.Unterricht;
using System.Linq;

namespace SvwsIccImporter.Service
{
    public class IdResolver : IIdResolver
    {
        public string ResolveIdForStudyGroup(Unterricht unterricht)
        {
            if (unterricht.KursId == null)
            {
                return string.Join('-', unterricht.Klassen.Select(x => x.Name).Distinct().OrderBy(x => x));
            }

            return unterricht.KursId.Value.ToString();
        }

        public string ResolveIdForTuition(Unterricht unterricht)
        {
            if(unterricht.KursId != null)
            {
                return unterricht.KursId.Value.ToString();
            }

            return string.Join('-', unterricht.Fach.Kuerzel, ResolveIdForStudyGroup(unterricht));
        }
    }
}
