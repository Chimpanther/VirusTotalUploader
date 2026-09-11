using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

public class MockHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"response_code\": 0}")
        });
    }
}

class Program {
    static async Task Main() {
        var options = new RestClientOptions("https://example.com")
        {
            ConfigureMessageHandler = _ => new MockHandler()
        };
        var client = new RestClient(options);
        var req = new RestRequest("test");
        var res = await client.ExecuteAsync(req);
        Console.WriteLine(res.Content);
    }
}
