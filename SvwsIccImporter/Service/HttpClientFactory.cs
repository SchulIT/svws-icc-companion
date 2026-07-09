using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SvwsIccImporter.Service
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateHttpClient(string username, string password, bool ignoreCertificateWarnings)
        {
            var client = new HttpClient();

            if (ignoreCertificateWarnings)
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => { return true; };

                client = new HttpClient(handler);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            return client;
        }
    }
}
