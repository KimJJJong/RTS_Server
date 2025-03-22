using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AuthClient
{
    private readonly HttpClient _httpClient;

    public AuthClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    // 회원가입 요청 메서드
    public async Task<RegisterResponse> RegisterAsync(string email, string password)
    {
        var registerDto = new RegisterDto { email = email, password = password };
        string json = JsonSerializer.Serialize(registerDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync("/users/register", content);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"회원가입 실패: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return registerResponse;
    }

    // 로그인 요청 메서드
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var loginDto = new LoginDto { email = email, password = password };

        // JSON 직렬화
        string json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // API 엔드포인트를 "/users/login"으로 수정
        HttpResponseMessage response = await _httpClient.PostAsync("/users/login", content);

        // 응답 상태 코드가 성공이 아닌 경우 처리
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"로그인 실패: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }

        // 응답을 JSON으로 역직렬화하여 LoginResponse 객체에 저장
        string responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return loginResponse;
    }
}
