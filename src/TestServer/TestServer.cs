using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using InternalTestServer = Microsoft.AspNetCore.TestHost.TestServer;

namespace Dubstep.TestUtilities
{
    public class TestServer
    {
        public RuleSet CurrentRuleSet { get; private set; }

        public TestServer()
        {
            CurrentRuleSet = new RuleSet();
        }

        /// <summary>
        /// Generate a HttpClient instance
        /// </summary>
        public HttpClient CreateClient()
        {
            return CreateClient(null);
        }

        /// <summary>
        /// Generate a HttpClient instance and applies additional DelegatingHandler on this client instance
        /// </summary>
        public HttpClient CreateClient(DelegatingHandler handler)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(CurrentRuleSet);
                })
                .UseStartup<Startup>();
            var testServer = new InternalTestServer(builder);
            var innerHttpClient = testServer.CreateClient();

            if (handler == null)
            {
                return innerHttpClient;
            }

            var httpMessageHandler = new HttpClientWrapHandler(innerHttpClient, handler);
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = innerHttpClient.BaseAddress
            };
            return httpClient;
        }
    }
}
