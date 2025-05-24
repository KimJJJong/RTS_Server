using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shared;

public class HttpServer
{
    private IHost? _httpHost;

    // HTTP API 서버 항상 실행 (비동기)
    public async Task Start(int port)
    {
        string localIp = "0.0.0.0";

        _httpHost = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls($"http://{localIp}:{port}");
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost("/match", HandleMatchRequest);
                    });
                });
            })
            .Build();

        await _httpHost.RunAsync();
    }

    // 매칭 요청 처리 (HTTP POST)
    private async Task HandleMatchRequest(HttpContext context)
    {
        var request = await context.Request.ReadFromJsonAsync<MatchRequest>();
        if (request?.Players.Count == 2)
        {
            RedisMatchQueue.Enqueue(request.Players);
            var roomId = Guid.NewGuid().ToString().Substring(0, 5);
            await context.Response.WriteAsJsonAsync(new { success = true, roomId });
            Console.WriteLine($"[HTTP API] Enqueued match for players: {string.Join(", ", request.Players)}");
        }
        else
        {
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid Match Request" });
            Console.WriteLine("[HTTP API] Invalid Match Request");
        }
    }
}

// HTTP API 매칭 요청 클래스
public class MatchRequest
{
    public List<int> ?Players { get; set; }
}
