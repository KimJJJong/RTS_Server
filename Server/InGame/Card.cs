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
    public string ProjectileCardID; // 없으면 null
}


public static class CardMetaDatabase
{
    private static Dictionary<(string, int), CardMeta> _metas = new Dictionary<(string, int), CardMeta>();

    public static void Load()
    {
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
            ProjectileCardID = "PRJ-U-JOS-004"
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

        // ❗️기타 유닛은 추가적으로 여기에 계속 넣으면 됨
    }

    public static CardMeta GetMeta(string cardID, int level)
    {
        if (_metas.TryGetValue((cardID, level), out var meta))
            return meta;

        return null;
    }
}

