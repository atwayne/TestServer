using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dubstep.TestUtilities.Tests
{
    public class TestServerTests
    {
        private TestServer _server;
        private const string _okResponse = "[1,2]";

        private void AssertOkResponse(HttpResponseMessage response, string expectedContent = _okResponse)
        {
            response.EnsureSuccessStatusCode();
            var message = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(expectedContent, message);
        }

        private void AssertNoPredictionMatched(HttpResponseMessage response)
        {
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private void AssertBadRequest(HttpResponseMessage response)
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [SetUp]
        public void SetUp()
        {
            _server = new TestServer();
        }

        [Test]
        public async Task WhenPredictionIsEmpty_GetAsync_ShouldSucceed()
        {
            // Arrange
            _server.CurrentRuleSet
                .AddRule()
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.GetAsync("/");

            // Assert
            AssertOkResponse(actual);
        }

        [Test]
        public async Task WhenPredictionIsEmpty_PostAsync_ShouldSucceed()
        {
            // Arrange
            _server.CurrentRuleSet
                .AddRule()
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.PostAsync("/", null);

            // Assert
            AssertOkResponse(actual);
        }

        [Test]
        public async Task WhenPredictGet_GetAsync_ShouldSucceed()
        {
            // Arrange
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.GetAsync("/");

            // Assert
            AssertOkResponse(actual);
        }

        [Test]
        public async Task WhenPredictPost_PostAsync_ShouldSucceed()
        {
            // Arrange
            _server.CurrentRuleSet
                .AddRule()
                .WhenPost()
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.PostAsync("/", new StringContent("dummy"));

            // Assert
            AssertOkResponse(actual);
        }

        [Test]
        public async Task WhenPredictGet_PostAsync_ShouldFail()
        {
            // Arrange
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.PostAsync("/", null);

            // Assert
            AssertNoPredictionMatched(actual);
        }

        [Test]
        public async Task WhenPredictUrlMatch_GetAsync_ShouldSucceed()
        {
            // Arrange
            var urlPattern = "\\?id=1";
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .WhenUrlMatch(urlPattern)
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.GetAsync("/?id=1");

            // Assert
            AssertOkResponse(actual);
        }

        [Test]
        public async Task WhenPredictUrlNotMatch_ShouldFail()
        {
            // Arrange
            var urlPattern = "\\?id=1";
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .WhenUrlMatch(urlPattern)
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();

            // Act
            var actual = await client.PostAsync("/?id=2", null);

            // Assert
            AssertNoPredictionMatched(actual);
        }

        [Test]
        public async Task WhenPredictAuthorizationTokenMatch_ShouldSucceed()
        {
            // Arrange
            var authorizationSchema = "Bearer";
            var authorizationToken = "fancy-token";
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .WhenAuthorizationMatch($"{authorizationSchema} {authorizationToken}")
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = client.BaseAddress,
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);

            // Act
            var actual = await client.SendAsync(request);

            //Assert
            AssertOkResponse(actual);
        }

        [Test]
        public async Task WhenPredictAuthorizationTokenNotMatch_ShouldFail()
        {
            // Arrange
            var authorizationSchema = "Bearer";
            var authorizationToken = "fancy-token";
            var wrongToken = "expired-token";
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .WhenAuthorizationMatch($"{authorizationSchema} {authorizationToken}")
                .SetOkResponse(_okResponse);
            var client = _server.CreateClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = client.BaseAddress,
                Method = HttpMethod.Get,
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", wrongToken);

            // Act
            var actual = await client.SendAsync(request);

            //Assert
            AssertNoPredictionMatched(actual);
        }

        [Test]
        public async Task WhenSetBadRequestAsResponse_ShouldReturnBadRequest()
        {
            _server.CurrentRuleSet
                .AddRule()
                .WhenGet()
                .SetBadRequest();
            var client = _server.CreateClient();

            // Act
            var actual = await client.GetAsync("/");

            // Assert
            AssertBadRequest(actual);
        }

        [Test]
        public async Task WhenSecondRuleMatched_ShouldSucceed()
        {
            // Arrange
            var firstPattern = "\\?id=1";
            var firstResponse = "first rule matched";
            var secondPattern = "\\?id=2";
            var secondResponse = "second rule matched";

            _server.CurrentRuleSet
                .AddRule()
                .WhenUrlMatch(firstPattern)
                .SetOkResponse(firstResponse)
                .AddRule()
                .WhenUrlMatch(secondPattern)
                .SetOkResponse(secondResponse);

            var client = _server.CreateClient();

            // Act
            var actual = await client.GetAsync("?id=2");

            // Assert
            AssertOkResponse(actual, secondResponse);
        }


        [Test]
        public async Task WhenMutipleRulesMatched_ShouldActionOnFirstMatch()
        {
            // Arrange
            var firstPattern = "\\?id=1";
            var firstResponse = "first rule matched";
            var secondPattern = "\\?id=*";
            var secondResponse = "second rule matched";

            _server.CurrentRuleSet
                .AddRule()
                .WhenUrlMatch(firstPattern)
                .SetOkResponse(firstResponse)
                .AddRule()
                .WhenUrlMatch(secondPattern)
                .SetOkResponse(secondResponse);

            var client = _server.CreateClient();

            // Act
            var actual = await client.GetAsync("?id=1");

            // Assert
            AssertOkResponse(actual, firstResponse);
        }
    }
}