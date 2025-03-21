using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Server.Log
{
    /// <summary>
    /// 로그 단계(정보, 경고, 에러, 디버그)
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug
    }

    /// <summary>
    /// 하나의 로그 항목을 표현하는 클래스
    /// </summary>
    public class LogEntry
    {
        // 발생 시각, 로그 종류, 발생 위치, 메시지
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }

        // 파일에 저장할 형식 << 수정 가능
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] {Source} - {Message}";
        }
    }

    /// <summary>
    /// 로그 매니저 (싱글턴)
    /// 비동기 로그 큐를 사용하여 로그 파일 및 콘솔에 기록합니다.
    /// </summary>
    public class LogManager
    {
        // Lazy를 사용 >> 사용할 때만 로딩 >> 최적화 됨 와!
        private static readonly Lazy<LogManager> _instance = new Lazy<LogManager>(() => new LogManager());
        // 그냥 _instance 넣으면 Lazy<LogManager>가 반환됨 LogManager를 반환하기 위해서 .Value사용
        public static LogManager Instance => _instance.Value;

        // 비동기 로그 큐 (BlockingCollection을 사용하여 생산자/소비자 패턴 구현, 알아서 락 걸어줌)
        private readonly BlockingCollection<LogEntry> _logQueue = new BlockingCollection<LogEntry>();

        // 로그 파일 경로 (실행 파일 위치의 logs 폴더 아래에 생성)
        // 이건 나중에 수정하긴 해야될 듯, 호스팅하면 어케될지 진짜 모름 ㅋㅋ
        private readonly string _logFilePath;
        private volatile bool _running = true;

        private LogManager()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            // 폴더 없으면 생성
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logFilePath = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            // 별도의 스레드에서 로그 큐를 처리
            Task.Factory.StartNew(ProcessLogQueue, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 로그 기록
        /// </summary>
        /// <param name="level">로그 레벨</param>
        /// <param name="source">로그 발생 위치 또는 클래스명</param>
        /// <param name="message">로그 메시지</param>
        public void Log(LogLevel level, string source, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message
            };
            _logQueue.Add(entry);
        }

        /// <summary>
        /// 정보 로그 기록
        /// </summary>
        public void LogInfo(string source, string message) => Log(LogLevel.Info, source, message);

        /// <summary>
        /// 경고 로그 기록
        /// </summary>
        public void LogWarning(string source, string message) => Log(LogLevel.Warning, source, message);

        /// <summary>
        /// 에러 로그 기록
        /// </summary>
        public void LogError(string source, string message) => Log(LogLevel.Error, source, message);

        /// <summary>
        /// 디버그 로그 기록
        /// </summary>
        public void LogDebug(string source, string message) => Log(LogLevel.Debug, source, message);

        /// <summary>
        /// 로그 큐를 지속적으로 처리
        /// 파일, 콘솔에 기록함
        /// 개발 단계에서는 콘솔에 남기고 운영 환경에서는 진짜 중요한 거만 남기는 것이 좋다고 함
        /// </summary>
        private void ProcessLogQueue()
        {
            try
            {
                using (var streamWriter = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                {
                    // _running이 false일 때 = 서버 닫혔을 때
                    // 큐에 있는 로그들 전부 내보내고 끝
                    while (_running || _logQueue.Count > 0)
                    {
                        LogEntry entry;
                        try
                        {
                            entry = _logQueue.Take();
                        }
                        catch (InvalidOperationException)
                        {
                            break;
                        }
                        // 로그를 바로바로 기록하기 위해 flush 해줌
                        // 실무에서도 로그 손실 방지를 위해 바로바로 flush 한다고 함
                        streamWriter.WriteLine(entry.ToString());
                        streamWriter.Flush();

                        // 필요할 땐 콘솔에 출력
                        Console.WriteLine(entry.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 처리 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 서버 종료 시 호출
        /// </summary>
        public void Shutdown()
        {
            _running = false;
            _logQueue.CompleteAdding();
        }
    }
}
