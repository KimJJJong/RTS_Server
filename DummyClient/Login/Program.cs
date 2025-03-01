/*using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        string serverUrl = "http://13.124.197.216:3000"; // 실제 URL로 대체
        var client = new AuthClient(serverUrl);

        Console.WriteLine("회원가입을 진행하시겠습니까? (Y/N)");
        string registerInput = Console.ReadLine();

        if (registerInput?.ToLower() == "y")
        {
            Console.Write("이메일을 입력하세요: ");
            string email = Console.ReadLine();

            Console.Write("비밀번호를 입력하세요: ");
            string password = Console.ReadLine();

            var registerResult = await client.RegisterAsync(email, password);

            if (registerResult != null)
            {
                Console.WriteLine("회원가입 성공!");
                Console.WriteLine("메시지: " + registerResult.message);
                Console.WriteLine("사용자 ID: " + registerResult.userId);
            }
            else
            {
                Console.WriteLine("회원가입에 실패했습니다.");
                return;
            }
        }

        Console.WriteLine("\n로그인을 진행합니다.");

        Console.Write("이메일을 입력하세요: ");
        string loginEmail = Console.ReadLine();

        Console.Write("비밀번호를 입력하세요: ");
        string loginPassword = Console.ReadLine();

        var loginResult = await client.LoginAsync(loginEmail, loginPassword);

        if (loginResult != null)
        {
            Console.WriteLine("로그인 성공!");
            Console.WriteLine("Access Token: " + loginResult.access_token);
            Console.WriteLine("Game Server Host: " + loginResult.gameServer.host);
            Console.WriteLine("Game Server Port: " + loginResult.gameServer.port);

            // 이후 게임 서버에 연결하는 로직을 추가할 수 있습니다.
        }
        else
        {
            Console.WriteLine("로그인에 실패했습니다.");
        }
    }
}
*/