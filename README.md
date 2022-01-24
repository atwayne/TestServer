# TestServer

`TestServer` is designed for unit test. It generates an `HttpClient` instance that you can inject to your service.

## Get Started

1. Install the package by nuget
    ```bash
    dotnet add package Dubstep.TestUtilities.TestServer
    ```
2. Create a `TestServer` instance by `new TestServer()`
3. Configure `RuleSet` of the `TestServer` instance
4. Generate a HttpClient by `CreateClient()`

## Example
```csharp
// The default vaule when not match found is a HttpNotFound response
// It can be changed by SetDefaultAction
server.CurrentRuleSet
    .SetDefaultAction(async (response) =>
    {
        response.StatusCode = 500;
        await response.WriteAsync("Simon says server error");
    });

// Return OK response for every request
server.CurrentRuleSet
    .AddRule()
    .SetOkResponse("ok");

// Return Bad Request for every request
server.CurrentRuleSet
    .AddRule()
    .SetBadRequest();

// Return OK response for every HTTP GET request
server.CurrentRuleSet
    .AddRule()
    .WhenGet()
    .SetOkResponse("ok");

// Return OK response for every HTTP Get request that matches an url patter
var urlPattern = "\\?id=1";
server.CurrentRuleSet
    .AddRule()
    .WhenUrlMatch(urlPattern)
    .SetOkResponse("ok")

// Return OK response for every HTTP request that has expected header
server.CurrentRuleSet
    .AddRule()
    .WhenHeaderMatch("User-Agent", expectedUserAgent)
    .SetOkResponse("ok");

// You can chain your configuration, if the request matches mutiple rules, the first match will be picked
var firstPattern = "\\?id=1";
var secondPattern = "\\?id=*";

server.CurrentRuleSet
    .AddRule()
    .WhenUrlMatch(firstPattern)
    .SetOkResponse("ok-1")
    .AddRule()
    .WhenUrlMatch(secondPattern)
    .SetOkResponse("ok-2");

// Expire the rule after a certain count of match, once the rule has expired, it will never match a request
server.CurrentRuleSet
    .AddRule()
    .SetMaxMatchCount(1)
    .SetOkResponse(okResponse);

// Return Ok response on the first request, and Ok response with a different content on the second request
server.CurrentRuleSet
    .AddRule()
    .SetMaxMatchCount(1)
    .SetOkResponse("ok-1");
    .AddRule()
    .SetOkResponse("ok-2");

// Generate a HttpClient instance
var httpClient = server.CreateClient();

// Apply a HttpMessageHandler to the client
var httpClient = server.CreateClient(new AddAuthorizationHandler());
```

Check [the unit test cases](https://github.com/atwayne/TestServer/blob/master/test/TestServer.Tests/TestServerTests.cs) for more examples
