using System;
using System.Collections.Generic;


    public class GameRoomFactory
    {
        public static GameRoom CreateRoom(string roomId)
        {
            return new GameRoom(roomId);
        }
    }
