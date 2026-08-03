using System;
using System.Threading.Tasks;
using RestSharp;

class Program {
    static async Task Main() {
        var client = new RestClient("http://127.0.0.1:8080");
        var req = new RestRequest("test", Method.Post);
        var resp = await client.ExecuteAsync(req);
        Console.WriteLine(resp.Content);
    }
}
