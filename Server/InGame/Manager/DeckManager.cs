using Server;
using ServerCore;
using Shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

class DeckManager
{
    private Dictionary<int, List<Card>> _playerDecks = new Dictionary<int, List<Card>>();
    private List<Card> _cardPool = new List<Card>();
    private List<Card> _playerDeckList = new List<Card>();
    List<Card> allCards = new List<Card>();

    public void Init(List<Card> deckList)
    {
        /*     _playerDecks.Clear();
             _cardPool.Clear();

             foreach (var session in clientSessions)
             {
                 int id = session.SessionID;
                 var deck = session.OwnDeck ?? new List<Card>();

                 _playerDecks[id] = new List<Card>(deck);
                 _cardPool.AddRange(deck);
             }

             Console.WriteLine("CardPool is ready, ");*/
        _playerDeckList.Clear();
        _playerDeckList = deckList;
        SetAllCards();
    }

    public List<Card> GetAllCards()
    {
        return allCards;
    }
    public List<Card> SetAllCards()
    {
        allCards.Clear();

        // WallMaria 추가
        List<Card> castles = new List<Card>() { new Card("CASTLE-U-01", 1), new Card("CASTLE-U-02", 1) };
        allCards.AddRange(castles);

        // Card Object 추가
            allCards.AddRange(_playerDeckList);

        // 파생 데미지 관련 추가
        AppendProjectileCards(allCards);

        foreach(Card card in allCards)
        {
            Console.WriteLine($"Card with Object ID :[{card.ID}]  Lv : [ {card.LV} ]");
        }

        return allCards;
    }

    private void AppendProjectileCards(List<Card> cards)
    {
        List<Card> extraProjectiles = new List<Card>();

        foreach (Card card in cards)
        {

            CardMeta meta = CardMetaDatabase.GetMeta(card.ID, card.LV);
            //Console.WriteLine($"card : {card.ID} meta : [{meta.CardID} || {meta.ProjectileCardID}] ");
            if (meta != null && meta.IsRanged && !string.IsNullOrEmpty(meta.ProjectileCardID))
            {
                extraProjectiles.Add(new Card(meta.ProjectileCardID, card.LV));
                //LogManager.Instance.LogInfo("DeckManager", $"[Projectile Add] {meta.ProjectileCardID} for {card.ID}");
            }
        }

        cards.AddRange(extraProjectiles);
    }


    //public S_CardPool MakeCardPoolPacket()
    //{
    //    S_CardPool packet = new S_CardPool();
    //    packet.size = 10;   // TODO : 야...이거왜 size가 UnitPoolManager에 있냐... 맞긴 한데...그래;;;
    //    foreach (var card in _cardPool)
    //    {
    //        packet.cardCombinations.Add(new S_CardPool.CardCombination
    //        {
    //            uid = card.ID,
    //            lv = card.LV
    //        });
    //    }

    //    return packet;
    //}

    public void Clear()
    {
        _playerDecks.Clear();
        _cardPool.Clear();
    }

    public List<Card> CardPoolList => _cardPool;
}
