using Server;
using System;
using System.Collections.Generic;

class TileManager
{
    private const int Width = 8;   // 가로 (X축)
    private const int Height = 12; // 세로 (Y축)

    private TileState[,] _tiles = new TileState[Width, Height];
    private OccupationManager _occupationManager;
    private PositionCache _positionCache;

    private int player0;
    public TileManager(OccupationManager occupationManager, PositionCache positionCache)
    {
        _occupationManager = occupationManager;
        _positionCache = positionCache;


    }

    public void Init(List<int> sessionIdList)
    {
        var player0 = sessionIdList[0];
        var player1 = sessionIdList[1];

        this.player0 = player0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _tiles[x, y] = new TileState(x, y);

                if (y < Height / 2)
                    _tiles[x, y].Claim(player0);
                else
                    _tiles[x, y].Claim(player1);
            }
        }
    }
    public bool TryCaptureFromClient(ClientSession session, C_TileClaimReq packet)
    {
        var (serverX, serverY) = _positionCache.ClientToServer(session.SessionID, packet.x, packet.y);
        return TryCaptureTile(serverX, serverY, session.SessionID);
    }

    public bool TryCaptureTile(int x, int y, int sessionId)
    {
        if (!IsValidCoord(x, y)) return false;

        TileState tile = _tiles[x, y];
        if (tile.OwnerSessionId == sessionId) return false;   //이미 내가 점령 하구 있다구~

        tile.Claim(sessionId);
        _occupationManager.OnTileClaim(sessionId, x, y);

        //    Console.WriteLine($"[TileManager] Tile Captured: ({x}, {y}) by Session {sessionId}");
        return true;
    }

    private bool IsValidCoord(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public void TryBulkClaimTile(int sessionId, List<(int x, int y)> tilesPositions)
    {
        List<(int x, int y)> claimedTiles = new List<(int x, int y)>();

        foreach (var (x, y) in tilesPositions)
        {
            if (!IsValidCoord(x, y)) continue;

            TileState tile = _tiles[x, y];

            // 이미 내꺼지롱~~~
            if (tile.OwnerSessionId == sessionId) continue;

            tile.Claim(sessionId);
            claimedTiles.Add((x, y));
        }

        if (claimedTiles.Count > 0)
            _occupationManager.OnBulkTileClaim(sessionId, tilesPositions);
    }

    public void Clear()
    {
        // TODO: 필요 시 초기화 처리
    }
}
