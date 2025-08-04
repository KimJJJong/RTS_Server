using System;
using System.Net;
using System.Threading.Tasks;
using ServerCore;
using Shared;

namespace Server
{
    class Program
    {
        static Listener _clientListener = new Listener();
        static Listener _matchingListener = new Listener();

        static void Main(string[] args)
        {
            // 프로그램 종료 시 로그 매니저 종료
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                LogManager.Instance.Shutdown();
            };

            //TCP Listener (내부 매칭 서버 연결)
           /* string mHost = Dns.GetHostName();
            IPHostEntry mIpHost = Dns.GetHostEntry(mHost);
            IPEndPoint mEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.2"), 13222);
*/

            // TCP Listener (클라이언트 연결 처리)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.112.119")/*ipAddr*/, 13221);


            _clientListener.Init(endPoint, () => { return SessionManager.Instance.Generate<ClientSession>(); });
            LogManager.Instance.LogInfo("Program", "[Game Server Start]");
            Console.WriteLine("ClientSession Listening...");

  /*          _matchingListener.Init(mEndPoint, () => { return SessionManager.Instance.Generate<MatchSession>(); } );
            LogManager.Instance.LogInfo("Program", "[Matching Stand By Start]");
            Console.WriteLine("MatchingSession Listening...");

*/

            CardMetaDatabase.Load();
            UnitStatDatabase.Load();



            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
