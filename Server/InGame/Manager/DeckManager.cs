using Server;
using Shared;
using System.Collections.Generic;

class DeckManager
{
    private Dictionary<int, List<Card>> _playerDecks = new Dictionary<int, List<Card>>();
    private List<Card> _cardPool = new List<Card>();

    public bool ReceiveDeck(ClientSession session, C_SetCardPool packet)
    {
        if (!_playerDecks.ContainsKey(session.SessionID))
            _playerDecks[session.SessionID] = new List<Card>();

        _playerDecks[session.SessionID].Clear();

        foreach (var cardData in packet.cardCombinations)
            _playerDecks[session.SessionID].Add(new Card(cardData.uid, cardData.lv));

        return _playerDecks.Count == 2;
    }

    public List<Card> GetAllCards()
    {
        List<Card> allCards = new List<Card>();

        // WallMaria 추가
        List<Card> castles = new List<Card>() { new Card("CASTLE-U-01", 1), new Card("CASTLE-U-02", 1) };
        allCards.AddRange(castles);

        // Card Object 추가
        foreach (var deck in _playerDecks.Values)
            allCards.AddRange(deck);

        // 파생 데미지 관련 추가
        AppendProjectileCards(allCards);

        return allCards;
    }

    private void AppendProjectileCards(List<Card> cards)
    {
        List<Card> extraProjectiles = new List<Card>();

        foreach (Card card in cards)
        {
            CardMeta meta = CardMetaDatabase.GetMeta(card.ID, card.LV);
            if (meta != null && meta.IsRanged && !string.IsNullOrEmpty(meta.ProjectileCardID))
            {
                extraProjectiles.Add(new Card(meta.ProjectileCardID, card.LV));
                LogManager.Instance.LogInfo("DeckManager", $"[Projectile Add] {meta.ProjectileCardID} for {card.ID}");
            }
        }

        cards.AddRange(extraProjectiles);
    }

    public S_CardPool MakeCardPoolPacket()
    {
        S_CardPool packet = new S_CardPool();

        foreach (var card in GetAllCards())
        {
            packet.cardCombinations.Add(new S_CardPool.CardCombination
            {
                uid = card.ID,
                lv = card.LV
            });
        }

        return packet;
    }

    public void Clear()
    {
        _playerDecks.Clear();
        _cardPool.Clear();
    }
}
