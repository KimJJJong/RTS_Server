// 회원가입 요청 DTO
public class RegisterDto
{
    public string email { get; set; }
    public string password { get; set; }
}

// 로그인 요청 DTO
public class LoginDto
{
    public string email { get; set; }
    public string password { get; set; }
}

// 게임 서버 접속 정보
public class GameServerInfo
{
    public string host { get; set; }
    public int port { get; set; }
}

// 로그인 응답 DTO (JWT 토큰 및 게임 서버 정보 포함)
public class LoginResponse
{
    public string access_token { get; set; }
    public GameServerInfo gameServer { get; set; }
}

// 회원가입 응답 DTO
public class RegisterResponse
{
    public string message { get; set; }
    public int userId { get; set; }
}
