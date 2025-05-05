public class TileState
{
    public int X { get; }
    public int Y { get; }
    public int OwnerSessionId { get; private set; } = -1;

    public TileState(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Claim(int sessionId)
    {
        OwnerSessionId = sessionId;
    }
}
