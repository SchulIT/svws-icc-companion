using System.Net.Http;

namespace SvwsIccImporter.Service
{
    public interface IHttpClientFactory
    {
        public HttpClient CreateHttpClient(string username, string password, bool ignoreSslCertificate);
    }
}
