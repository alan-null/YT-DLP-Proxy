using System.Diagnostics;
using System.Collections.Concurrent;

var cache = new ConcurrentDictionary<string, string>();
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Urls.Add("http://*:497");

app.MapGet("/bestAudio", async (string v) =>
{
    if (string.IsNullOrEmpty(v))
    {
        return Results.BadRequest("Parameter 'v' is required.");
    }

    if (cache.TryGetValue(v, out var cachedUrl))
    {
        if (await IsUrlAccessible(cachedUrl))
        {
            return Results.Json(new { url = cachedUrl });
        }
        else
        {
            cache.TryRemove(v, out _);
        }
    }

    var url = $"https://www.youtube.com/watch?v={v}";
    var startInfo = new ProcessStartInfo
    {
        FileName = "yt-dlp",
        Arguments = $"--get-url -f bestaudio {url}",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using (var process = Process.Start(startInfo))
    {
        if (process == null)
        {
            return Results.BadRequest("yt-dlp not found.");
        }
        using (var reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();
            process.WaitForExit();

            cache[v] = result.Trim();

            return Results.Json(new { url = result.Trim() });
        }
    }
});

app.Run();

async Task<bool> IsUrlAccessible(string url)
{
    try
    {
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            return response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Forbidden;
        }
    }
    catch
    {
        return false;
    }
}
