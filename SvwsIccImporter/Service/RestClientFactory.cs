using SchulIT.SvwsClient;
using SvwsIccImporter.Settings;
using System;
using System.Net.Http;

namespace SvwsIccImporter.Service
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly ISettingsManager settingsManager;
        private readonly IHttpClientFactory httpClientFactory;

        public RestClientFactory(ISettingsManager settingsManager, IHttpClientFactory httpClientFactory)
        {
            this.settingsManager = settingsManager;
            this.httpClientFactory = httpClientFactory;
        }


        public SvwsRestClient GetClient()
        {
            if(string.IsNullOrEmpty(settingsManager.Settings.Svws.Server))
            {
                throw new ArgumentException("Server as not set.");
            }

            if(string.IsNullOrEmpty(settingsManager.Settings.Svws.Username))
            {
                throw new ArgumentException("Username was not set.");
            }

            if(settingsManager.Settings.Svws.Password == null)
            {
                throw new ArgumentException("Password was not set.");
            }

            return GetClient(settingsManager.Settings.Svws.Server, httpClientFactory.CreateHttpClient(settingsManager.Settings.Svws.Username, settingsManager.Settings.Svws.Password, settingsManager.Settings.Svws.IgnoreCertificateWarnings));
        }

        public SvwsRestClient GetClient(string baseUrl, HttpClient httpClient)
        {
            return new SvwsRestClient(baseUrl, httpClient)
            {
                ReadResponseAsString = true
            };
        }
    }
}
