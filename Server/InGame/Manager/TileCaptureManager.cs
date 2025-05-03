using System;

class TileCaptureManager
{
    private readonly OccupationManager _occupationManager;

    public TileCaptureManager(OccupationManager occupationManager)
    {
        _occupationManager = occupationManager;
    }

    public void HandleTileClaim(int sessionId)
    {
        _occupationManager.OnTileClaim(sessionId);
        Console.WriteLine($"[TileCapture] P{sessionId} 타일 점령 → 점령도 증가");
    }
}
