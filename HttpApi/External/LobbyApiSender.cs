using System.Text.Json;
using System.Text;

public class MatchCompleteRequest
{
//    public List<string> Players { get; set; }
    public string Player1 {  get; set; }
    public  string Player2 { get; set; }
    public string RoomId { get; set; }
       
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
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{lobbyUrl}/match/complete", content);
            response.EnsureSuccessStatusCode();
        }catch (Exception ex)
        {
            Console.WriteLine($"[Warning] Failed to notify lobby: {ex.Message}");
        }
    }

    public static async Task SendMatchResultAsync(string lobbyUrl, MatchResultRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{lobbyUrl}/match/result", content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"[Warning] Failed to notify lobby: {ex.Message}"); 
        }
    }
}
