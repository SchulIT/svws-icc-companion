using SvwsClient.Tool.Unterricht;

namespace SvwsIccImporter.Service
{
    public interface INameResolver
    {
        public string ResolveName(Unterricht unterricht);
    }
}
