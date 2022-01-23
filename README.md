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
public async Task ShouldGetResponse {
    // Arrange
    var server = new TestServer();
    var urlPattern = "\\?id=1";
    var okResponse = "[1,2,3]";
    server.CurrentRuleSet
        .AddRule()
        .WhenGet()
        .WhenUrlMatch(urlPattern)
        .SetOkResponse(okResponse);
    var client = server.CreateClient();

    // Act
    var response = await client.GetAsync("/?id=1");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var message = await response.Content.ReadAsStringAsync();
    Assert.AreEqual(okResponse, message);
}
```

Check [the unit test cases](https://github.com/atwayne/TestServer/blob/master/test/TestServer.Tests/TestServerTests.cs) for more examples
