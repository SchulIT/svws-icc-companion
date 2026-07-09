using SchulIT.SvwsClient;
using SvwsClient.Tool.Unterricht;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SvwsIccImporter.Service
{
    public class CachedUnterrichtResolver : ICachedUnterrichtResolver
    {
        private List<Unterricht>? cache = null;

        private readonly UnterrichtResolver unterrichtResolver;

        public CachedUnterrichtResolver(UnterrichtResolver unterrichtResolver)
        {
            this.unterrichtResolver = unterrichtResolver;
        }

        public void ClearCache()
        {
            cache = null;
        }

        public async Task<List<Unterricht>> ResolveCachedAsync(SvwsRestClient client, string schema, long abschnittId)
        {
            if (cache == null)
            {
                cache = await unterrichtResolver.Resolve(client, schema, abschnittId);
            }

            return new List<Unterricht>(cache);
        }
    }
}
