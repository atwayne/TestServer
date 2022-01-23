using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dubstep.TestUtilities.Tests
{
    /// <summary>
    /// A dummy http message handler for testing purpose
    /// </summary>
    internal class AddUserAgentHandler : DelegatingHandler
    {
        public AddUserAgentHandler()
        {
            InnerHandler = new HttpClientHandler();
        }
        public static string DefaultUserAgent => nameof(AddUserAgentHandler);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("User-Agent", DefaultUserAgent);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
