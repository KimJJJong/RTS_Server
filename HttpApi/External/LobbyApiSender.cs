using System.Text.Json;
using System.Text;

public class MatchCompleteRequest
{
    public string RoomId { get; set; }
    public List<string> Players { get; set; }
}


public class MatchResultRequest
{
    public string RoomId { get; set; }
    public string WinnerId { get; set; }
    public string LoserId {  get; set; }
}


public static class LobbyApiSender
{
    private static readonly HttpClient _client = new HttpClient();

    public static async Task SendMatchCompleteAsync(string lobbyUrl, MatchCompleteRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{lobbyUrl}/match/complete", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task SendMatchResultAsync(string lobbyUrl, MatchResultRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{lobbyUrl}/match/result", content);
        response.EnsureSuccessStatusCode();
    }
}
