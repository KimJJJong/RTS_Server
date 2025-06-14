using System;
using System.Collections.Generic;
using System.Net.Sockets;

    public class GameRoomManager
    {
        private static GameRoomManager _instance = new GameRoomManager();
        public static GameRoomManager Instance => _instance;

        private Dictionary<string, GameRoom> _rooms = new Dictionary<string, GameRoom>();

        // Room 생성 <HTTP API 호출>
        public string CreateRoom(List<string> players, List<Card> deckCombination)
        {
            string roomId = Guid.NewGuid().ToString().Substring(0, 5);
            GameRoom room = GameRoomFactory.CreateRoom(roomId, players, deckCombination);
            _rooms[roomId] = room;
            Console.WriteLine($"[GameRoomManager] Room {roomId} created for Players: {string.Join(", ", players)}  will be Join");
            return roomId;
        }

        // Room 찾기
        public GameRoom FindRoom(string roomId)
        {
            return _rooms.TryGetValue(roomId, out var room) ? room : null;
        }

        // Room 제거
        public void RemoveRoom(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                _rooms.Remove(roomId);
                Console.WriteLine($"[GameRoomManager] Room {roomId} removed");
            }
        }
    }
