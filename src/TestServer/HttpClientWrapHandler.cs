using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Dubstep.TestUtilities
{
    internal class HttpClientWrapHandler : DelegatingHandler
    {
        private HttpClient _httpClient;
        public HttpClientWrapHandler(HttpClient httpClient, HttpMessageHandler innerHandler)
        {
            _httpClient = httpClient;
            InnerHandler = innerHandler;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var source = new CancellationTokenSource();
            var cancelledToken = source.Token;
            source.Cancel();
            try
            {
                await base.SendAsync(request, cancelledToken);
            }
            catch { }

            // Reset the request status and send it again with the inner httpClient
            ResetSendStatus(request);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        private void ResetSendStatus(HttpRequestMessage request)
        {
            // https://stackoverflow.com/a/43512592/553073
            var SEND_STATUS_FIELD_NAME = "_sendStatus";
            var requestType = request.GetType().GetTypeInfo();
            var sendStatusField = requestType.GetField(SEND_STATUS_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
            if (sendStatusField != null)
                sendStatusField.SetValue(request, 0);
            else
                throw new Exception($"Failed to hack HttpRequestMessage, {SEND_STATUS_FIELD_NAME} doesn't exist.");
        }
    }
}
