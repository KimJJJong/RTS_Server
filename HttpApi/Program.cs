using System;
using System.Net;
using System.Threading.Tasks;
using ServerCore;
using Shared;

    class Program
    {
        static HttpServer _httpServer;

        static async Task Main(string[] args)
        {


            // HTTP API 서버 실행 (로비 서버로부터 매칭 수신)
            _httpServer = new HttpServer();
            await _httpServer.Start(13222);


        }
    }