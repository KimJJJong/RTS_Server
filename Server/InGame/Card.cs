using System.Collections.Generic;

public class Card
{
    public string ID;
    public int LV;
    public Card(string ID, int LV)
    {
        this.ID = ID;
        this.LV = LV;
    }
}


public class CardMeta
{
    public string CardID;
    public int Level;

    public bool IsRanged;
    public string ProjectileCardID;

    public bool IsSpell;
    public List<string> SpellTimerIDs = new List<string>();      // ✅ 여러 타이머 참조
    public List<string> SpellPositionIDs = new List<string>();   // ✅ 여러 소환 범위 참조
    public string SpellType;                        // "PNT", "SUP" 등
    public float SpawnMana;
}


public static class CardMetaDatabase
{
    private static Dictionary<(string, int), CardMeta> _metas = new Dictionary<(string, int), CardMeta>();

    public static void Load()
    {
        //////////////////////////////////////////Projectile//////////////////////////////////////////
        // Scholar - U-JOS-003 → PRJ-U-JOS-003
        _metas[("U-JOS-003", 1)] = new CardMeta
        {
            CardID = "U-JOS-003",
            Level = 1,
            IsRanged = true,
            ProjectileCardID = "PRJ-U-JOS-003"
        };

        // Painter - U-JOS-004 → PRJ-U-JOS-004
        _metas[("U-JOS-004", 1)] = new CardMeta
        {
            CardID = "U-JOS-004",
            Level = 1,
            IsRanged = true,
            ProjectileCardID = "PRJ-U-JOS-003"  // tmp modife
        };

        // Crossbowman - U-ENG-003 → PRJ-U-ENG-003
        _metas[("U-ENG-003", 1)] = new CardMeta
        {
            CardID = "U-ENG-003",
            Level = 1,
            IsRanged = true,
            ProjectileCardID = "PRJ-U-ENG-003"
        };

        // Tower - ChromaticCannon → PRJ-TWR-ATK-001
        _metas[("TWR-ATK-001", 1)] = new CardMeta
        {
            CardID = "TWR-ATK-001",
            Level = 1,
            IsRanged = true,
            ProjectileCardID = "PRJ-TWR-ATK-001"
        };

        //////////////////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////////////Spell//////////////////////////////////////////
        {
            _metas[("SP-PNT-001", 1)] = new CardMeta
            {
                CardID = "SP-PNT-001",
                Level = 1,
                IsSpell = true,
                SpellType = "PNT",
                SpawnMana = 3,
                SpellTimerIDs = new List<string> { "TMR-SP-001" },
                SpellPositionIDs = new List<string> { "POS-SP-PNT-001" }
            };

            _metas[("SP-SUP-001", 1)] = new CardMeta
            {
                CardID = "SP-SUP-001",
                Level = 1,
                IsSpell = true,
                SpellType = "SUP",
                SpawnMana = 4,
                SpellTimerIDs = new List<string>(), // 비어있을 수도 있음
                SpellPositionIDs = new List<string> { "POS-SP-SUP-001" }
            };


            /////////////////////////////////////////////////////////////////////////////////////////

        }

    }
        public static CardMeta GetMeta(string cardID, int level)
        {
            if (_metas.TryGetValue((cardID, level), out var meta))
                return meta;

            return null;
        }
}

