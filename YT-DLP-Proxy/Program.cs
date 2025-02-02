using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Urls.Add("http://*:497");

app.MapGet("/bestaudio", (string v) =>
{
    if (string.IsNullOrEmpty(v))
    {
        return Results.BadRequest("Parameter 'v' is required.");
    }

    var url = $"https://www.youtube.com/watch?v={v}";
    var startInfo = new ProcessStartInfo
    {
        FileName = "yt-dlp",
        Arguments = $"--get-url -f bestaudio \"{url}\"",
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
            return Results.Json(new { url = result.Trim() });
        }
    }
});

app.Run();
