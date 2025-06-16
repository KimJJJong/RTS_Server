using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Linq;

class OccupationManager
{
    private const float MaxOccupation = 100f;
    private const float TileClaimScore = 1f;
    private const float WallHitScore = 0.35f;
    private const float InstantWinThreshold = 80f;

    private const int WallHitDelay = 2;

    private Dictionary<int, float> _occupation = new Dictionary<int, float>();  // sessionId -> 점령도
    private List<int> _playerSessionIds = new List<int>();         // 0번, 1번 플레이어 순서대로
    private GameLogicManager _logic;
    private PositionCache _positionCache;
    private TickManager _tickManager;
    public OccupationManager(GameLogicManager logic, PositionCache positionCache, TickManager tickManager)
    {
        _logic = logic;
        _positionCache = positionCache;
        _tickManager = tickManager;
    }

    public void Init(List<int> sessionIdList)
    {
        if (sessionIdList.Count != 2)
            throw new Exception("OccupationManager는 정확히 2명의 플레이어만 지원합니다.");

        _playerSessionIds = sessionIdList;
        _occupation[sessionIdList[0]] = 50f;
        _occupation[sessionIdList[1]] = 50f;

        Console.WriteLine($"[Occupation] {sessionIdList[0]}: {_occupation[sessionIdList[0]]:0.00}, {sessionIdList[1]}: {_occupation[sessionIdList[1]]:0.00}");
    }

    public int[] GetPlayerSessionIds() => _playerSessionIds.ToArray();

    public void OnTileClaim(int sessionId, int x, int y)
    {
        int opponent = GetOpponent(sessionId);
        AddScore(sessionId, opponent, TileClaimScore);

        var ids = GetPlayerSessionIds();
        int player0 = ids[0];

        foreach (int target in ids)
        {
            var (clientX, clientY) = _positionCache.ServerToClient(target, x, y);

            S_TileClaimed packet = new S_TileClaimed()
            {
                x = clientX,
                y = clientY,

                excutionTick = _tickManager.GetCurrentTick() + WallHitDelay,

                playerSession = sessionId,
                playerOccupation = GetOccupation(sessionId),
                opponentOccupation = GetOccupation(opponent)
            };
            //Console.WriteLine($"Send To {target} [X : {clientX} / Y : {clientY} ]");
            _logic.SendToPlayer(target, packet.Write());

            // ToDo : Client Tile 동기화
        }
    }
    public void OnBulkTileClaim(int sessionId, List<(int x, int y)> tilePositions)
    {
        int opponet = GetOpponent(sessionId);
        float totalBulkScore = TileClaimScore * tilePositions.Count;

        var ids = GetPlayerSessionIds();
        int player0 = ids[0];

        AddScore(sessionId, opponet, totalBulkScore);

        foreach (int target in GetPlayerSessionIds())
        {
            List<S_TileBulkClaimed.TileBulk> tileInfo = new List<S_TileBulkClaimed.TileBulk>();

            foreach (var (x, y) in tilePositions)
            {
                var (clientX, clientY) = _positionCache.ServerToClient(target, x, y);
                tileInfo.Add(new S_TileBulkClaimed.TileBulk
                {
                    x = clientX,
                    y = clientY,
                    claimedBySessionId = sessionId
                });
            }

            S_TileBulkClaimed packet = new S_TileBulkClaimed()
            {
                tileBulks = tileInfo,
                occupationRate = GetOccupation(sessionId),
                excutionTick = _tickManager.GetCurrentTick() + WallHitDelay,
                ReqPlayerSessionId = sessionId,
            };

            _logic.SendToPlayer(target, packet.Write());
        }
    }
    public void OnWallHit(int sessionId)
    {
        int opponent = GetOpponent(sessionId);
        AddScore(sessionId, opponent, WallHitScore);
        var ids = GetPlayerSessionIds();
        int player0 = ids[0];

        foreach (int target in ids)
        {
            S_OccupationSync pack = new S_OccupationSync()
            {
                playerSession = sessionId,
                excutionTick = _tickManager.GetCurrentTick() + WallHitDelay
                ,
                playerOccupation = GetOccupation(sessionId),
                opponentOccupation = GetOccupation(opponent)
            };
            _logic.SendToPlayer(target, pack.Write());

            // ToDo : Client측 점령도 동기화
        }
    }

    private void AddScore(int playerId, int opponentId, float amount)
    {
        _occupation[playerId] += amount;
        _occupation[opponentId] -= amount;
        Clamp(playerId, opponentId);

        //  Console.WriteLine($"[Occupation] {playerId}: {_occupation[playerId]:0.00}, {opponentId}: {_occupation[opponentId]:0.00}");

        if (_occupation[playerId] >= InstantWinThreshold)
        {
            Console.WriteLine($"[Occupation] 승리 조건 달성: {playerId} 즉시 승리!");
            _logic.EndGame(winnerSessionId: playerId);
        }
    }

    private void Clamp(int p1, int p2)
    {
        _occupation[p1] = Math.Clamp(_occupation[p1], 0, MaxOccupation);
        _occupation[p2] = Math.Clamp(_occupation[p2], 0, MaxOccupation);

        float total = _occupation[p1] + _occupation[p2];
        if (Math.Abs(total - MaxOccupation) > 0.01f)
        {
            float correction = (MaxOccupation - total) / 2f;
            _occupation[p1] += correction;
            _occupation[p2] += correction;
        }
    }

    public void CheckFinalWinner()
    {
        var sorted = _occupation.OrderByDescending(kvp => kvp.Value).ToList();
        if (sorted[0].Value > sorted[1].Value)
        {
            _logic.EndGame(sorted[0].Key);
            Console.WriteLine($"Player {sorted[0].Key} Win");
        }
        else
        {
            _logic.EndGame(-1); // 무승부
            Console.WriteLine("비김 ㅋ");
        }
    }

    public float GetOccupation(int sessionId)
    {
        return _occupation.TryGetValue(sessionId, out var val) ? val : 0f;
    }

    private int GetOpponent(int sessionId)
    {
        return _occupation.Keys.FirstOrDefault(id => id != sessionId);
    }

    public void Clear()
    {
        _occupation.Clear();
        _playerSessionIds.Clear();
    }
}
