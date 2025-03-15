using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DummyClient
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            string authServerUrl = "https://leeyoungwoo.shop"; // Auth 서버 주소
            var client = new AuthClient(authServerUrl);

            Console.WriteLine("\n로그인을 진행합니다.");
            Console.Write("이메일을 입력하세요: ");
            string loginEmail = Console.ReadLine();

            Console.Write("비밀번호를 입력하세요: ");
            string loginPassword = Console.ReadLine();

            try
            {
                var loginResult = await client.LoginAsync(loginEmail, loginPassword);
                 if (loginResult != null)
            {
                Console.WriteLine("로그인 성공!");
                Console.WriteLine("Access Token: " + loginResult.access_token);

                // GameServer에 JWT 인증 후 연결 시도
                await ConnectToGameServer("Test땐 안쓸거임", 13221, loginResult.access_token);
            }
            else
            {
                Console.WriteLine("로그인에 실패했습니다.");
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoginAsync 예외 발생: {ex.Message}");
            }

           
        }

        private static async Task ConnectToGameServer(string host, int port, string accessToken)
        {
            // DNS (Domain Name System)
            string _host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(_host);
            IPAddress ipAddr = ipHost.AddressList[0];

            IPEndPoint endPoint = new IPEndPoint(ipAddr, port);
            Connector connector = new Connector();

            connector.Connect(endPoint, () => {
                ServerSession session = SessionManager.Instance.Generate();
                session.AccessToken = accessToken;
                // 인증 토큰을 세션에 저장
                return session;
            });
            Console.WriteLine("게임 서버에 접속 시도 중...");

            while (true)
            {
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(250);
            }
        }
    }

    /*class Program
	{
		static void Main(string[] args)
		{
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(*//*IPAddress.Parse("172.30.1.5")*//*ipAddr, 13221);

			Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); } ,
				 1 );

			while (true)
			{
				try
				{
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(250);
			}
		}
	}*/
}
