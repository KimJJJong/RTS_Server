using Server;
using System;
using System.Collections.Generic;

class PlayerManager
{
    TickManager _tickManager;
    private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private Dictionary<int, Mana> _manas = new Dictionary<int, Mana>();

    public IReadOnlyDictionary<int, Mana> Manas => _manas;

    public PlayerManager(TickManager tickManager)
    {
        _tickManager = tickManager;
    }

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
            S_ManaUpdate packet = new S_ManaUpdate
            {
                currentMana = mana.Value.GetMana(),
                serverTick = _tickManager.GetCurrentTick()
            };
            
            _sessions[mana.Key].Send(packet.Write());
        }
    }
    public Mana GetMana(int sessionId)
    {
        _manas.TryGetValue(sessionId, out Mana mana);
        return mana;
    }
    public void SetManaRegenRate(float setRegenRate)
    {
        Console.WriteLine($"[PlayerManager] Set Mana RegenRate : {setRegenRate}");
        foreach (var playerManaInfo in _manas.Values)
            playerManaInfo.SetManaRegenRate(setRegenRate);
    }
    

    public void Clear()
    {
        _sessions.Clear();
        _manas.Clear();
    }
}
