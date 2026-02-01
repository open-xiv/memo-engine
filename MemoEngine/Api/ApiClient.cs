using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MemoEngine.Models;
using Newtonsoft.Json;


namespace MemoEngine.Api;

internal static class ApiClient
{
    private static readonly HttpClient Client;

    private static readonly string[] AssetUrls =
    [
        "https://assets.sumemo.dev",
        "https://haku.diemoe.net/assets"
    ];

    static ApiClient()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression  = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 4,
            UseProxy                = false
        };

        Client         = new HttpClient(handler);
        Client.Timeout = TimeSpan.FromSeconds(5);
    }

    public static async Task<DutyConfig?> FetchDuty(uint zoneId)
    {
        var tasks = AssetUrls.Select(assetUrl => FetchDutyFromUrl(assetUrl, zoneId)).ToList();
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

    private static async Task<DutyConfig?> FetchDutyFromUrl(string assetUrl, uint zoneId)
    {
        var       url = $"{assetUrl}/duty/{zoneId}";
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            var resp = await Client.GetAsync(url, cts.Token);
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return null;
                return null;
            }

            var content = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DutyConfig>(content);
        }
        catch (Exception) { return null; }
    }
}
