using System.Net;
using NovelParserBLL.Services.Interfaces;

namespace NovelParserBLL.Services;

public class WebClient : IWebClient
{
    private static readonly HttpClient client;
    private readonly int _maxRepeats;
    static WebClient()
    {
        var clientHandler = new HttpClientHandler
        {
            //Disable SSL verification
            ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        client = new HttpClient(clientHandler, true);
    }

    public WebClient(int requestRepeatsOnError = 4)
    {
        _maxRepeats = requestRepeatsOnError;
    }

    public async Task<string> GetStringContentAsync(string url, string agent = IWebClient.DefaultAgent,
        CancellationToken token = default)
    {
        var response = await GetResponseAsync(url, agent, token);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsStringAsync(token)
            : string.Empty;
    }

    public async Task<byte[]> GetBinaryContentAsync(string url, string agent = IWebClient.DefaultAgent,
        CancellationToken token = default)
    {
        var response = await GetResponseAsync(url, agent, token);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsByteArrayAsync(token)
            : Array.Empty<byte>();
    }
    
    private async Task<HttpResponseMessage> GetResponseAsync(string url, string agent = IWebClient.DefaultAgent, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", agent);
        var repeat = 0;

        HttpResponseMessage? response = null;
        do
        {
            try
            {
                response = await client.SendAsync(request, token);
            }
            catch (HttpRequestException)
            {
                repeat++;
            }
            if (response != null) break;
            await Task.Delay(500 * repeat, token);

        } while (repeat < _maxRepeats);

        return response ?? new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
