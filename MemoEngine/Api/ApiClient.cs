using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MemoEngine.Models;
using Newtonsoft.Json;


namespace MemoEngine.Api;

internal sealed class ApiClient : IDisposable
{
    private readonly HttpClient client;

    private readonly string[] assetUrls =
    [
        "https://assets.sumemo.dev",
        "https://haku.diemoe.net/assets"
    ];

    public ApiClient()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression  = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 4,
            UseProxy                = false
        };

        client         = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task<DutyConfig?> FetchDuty(uint zoneId)
    {
        var tasks = assetUrls.Select(assetUrl => FetchDutyFromUrl(assetUrl, zoneId)).ToList();
        while (tasks.Count > 0)
        {
            var complete = await Task.WhenAny(tasks);
            var result   = await complete;
            if (result is not null)
                return result;
            tasks.Remove(complete);
        }
        return null;
    }

    private async Task<DutyConfig?> FetchDutyFromUrl(string assetUrl, uint zoneId)
    {
        var       url = $"{assetUrl}/duty/{zoneId}";
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            var resp = await client.GetAsync(url, cts.Token);
            if (!resp.IsSuccessStatusCode)
                return null;

            var content = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DutyConfig>(content);
        }
        catch (Exception) { return null; }
    }

    public void Dispose()
        => client.Dispose();
}
