using SchulIT.SvwsClient;
using SvwsClient.Tool.Unterricht;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SvwsIccImporter.Service
{
    public interface ICachedUnterrichtResolver
    {
        public void ClearCache();

        public Task<List<Unterricht>> ResolveCachedAsync(SvwsRestClient client, string schema, long abschnittId);
    }
}
