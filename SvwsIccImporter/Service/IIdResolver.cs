using SvwsClient.Tool.Unterricht;

namespace SvwsIccImporter.Service
{
    public interface IIdResolver
    {
        public string ResolveIdForStudyGroup(Unterricht unterricht);

        public string ResolveIdForTuition(Unterricht unterricht);
    }
}
