using Server;
using System;
using System.Collections.Generic;
using System.Linq;

class PlayerManager
{
    TickManager _tickManager;

    private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    private Dictionary<int, Mana> _manas = new Dictionary<int, Mana>();
    public IReadOnlyDictionary<int, Mana> Manas => _manas;

    public void Init(IEnumerable<ClientSession> sessions, TickManager tickManager)
    {
        foreach (var session in sessions)
        {
            int id = session.PlayingID;
            _sessions[id] = session;
            _manas[id] = new Mana();
        }

        _tickManager = tickManager;
    }
    public void RegenManaAll()
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"[PlayerManager] : {ex}");
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
