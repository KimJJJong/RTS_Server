using System;
using System.Collections.Generic;


    public class GameRoomFactory
    {
        public static IGameRoom CreateRoom(string roomId, List<string> playerIds)
        {
            return new GameRoom(roomId, playerIds);
        }
    }
