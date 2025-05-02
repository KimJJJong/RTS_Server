using Server;
using System.Collections.Generic;

class PlayerManager
{
    private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private Dictionary<int, Mana> _manas = new Dictionary<int, Mana>();

    public IReadOnlyDictionary<int, Mana> Manas => _manas;
    public IReadOnlyDictionary<int, ClientSession> Sessions => _sessions;

    public void AddPlayer(ClientSession session)
    {
        _sessions[session.SessionID] = session;
        _manas[session.SessionID] = new Mana();
    }

    public void RegenManaAll()
    {
        foreach (var mana in _manas)
        {
            mana.Value.RegenMana();
            S_ManaUpdate packet = new S_ManaUpdate { currentMana = mana.Value.GetMana() };
            _sessions[mana.Key].Send(packet.Write());
        }
    }
    public Mana GetMana(int sessionId)
    {
        _manas.TryGetValue(sessionId, out Mana mana);
        return mana;
    }

    

    public void Clear()
    {
        _sessions.Clear();
        _manas.Clear();
    }
}
