using System;
using System.Collections.Generic;


    public class GameRoomFactory
    {
        public static GameRoom CreateRoom(string roomId, List<string> playerIds, List<Card> deckCombination)
        {
            return new GameRoom(roomId, playerIds, deckCombination);
        }
    }
