using SchulIT.SvwsClient;
using System.Net.Http;

namespace SvwsIccImporter.Service
{
    public interface IRestClientFactory
    {
        public SvwsRestClient GetClient();

        public SvwsRestClient GetClient(string baseUrl, HttpClient httpClient);
    }
}
