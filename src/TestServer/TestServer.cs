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
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(CurrentRuleSet);
                })
                .UseStartup<Startup>();
            var testServer = new InternalTestServer(builder);
            return testServer.CreateClient();
        }
    }
}
