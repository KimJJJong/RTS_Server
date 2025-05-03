using System;
using System.Collections.Generic;
using System.Linq;

class OccupationManager
{
    private const float MaxOccupation = 100f;
    private const float TileClaimScore = 1f;
    private const float WallHitScore = 0.25f;
    private const float InstantWinThreshold = 80f;

    private Dictionary<int, float> _occupation = new Dictionary<int, float>(); // Key = SessionID
    private GameLogicManager _logic;

    public OccupationManager(GameLogicManager logic)
    {
        _logic = logic;
    }

    public void Init(List<int> sessionIds)
    {
        if (sessionIds.Count != 2)
            throw new InvalidOperationException("Only 1vs1 supported for OccupationManager.");

        _occupation[sessionIds[0]] = 50f;
        _occupation[sessionIds[1]] = 50f;
    }

    public void OnTileClaim(int sessionId)
    {
        int opponent = GetOpponent(sessionId);
        AddScore(sessionId, opponent, TileClaimScore);
    }

    public void OnWallHit(int sessionId)
    {
        int opponent = GetOpponent(sessionId);
        AddScore(sessionId, opponent, WallHitScore);
    }

    private void AddScore(int playerId, int opponentId, float amount)
    {
        _occupation[playerId] += amount;
        _occupation[opponentId] -= amount;
        Clamp(playerId, opponentId);

        Console.WriteLine($"[Occupation] {playerId}: {_occupation[playerId]:0.00}, {opponentId}: {_occupation[opponentId]:0.00}");

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
        if (Math.Abs(total - MaxOccupation) > 0.001f)
        {
            float correction = (MaxOccupation - total) / 2f;
            _occupation[p1] += correction;
            _occupation[p2] += correction;
        }
    }

    public void CheckFinalWinner()
    {
        var kvp = _occupation.OrderByDescending(kvp => kvp.Value).ToList();
        if (kvp[0].Value > kvp[1].Value)
            _logic.EndGame(kvp[0].Key);
        else
            _logic.EndGame(-1); // 무승부
    }

    public float GetOccupation(int sessionId) => _occupation.ContainsKey(sessionId) ? _occupation[sessionId] : 0f;

    private int GetOpponent(int sessionId)
    {
        return _occupation.Keys.First(id => id != sessionId);
    }
    public void Clear()
    {
        _occupation.Clear();
    }
}
