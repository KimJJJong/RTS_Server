using System;
using System.Collections.Generic;
using System.Net.Sockets;

    public class GameRoomManager
    {
        private static GameRoomManager _instance = new GameRoomManager();
        public static GameRoomManager Instance => _instance;

        private Dictionary<string, IGameRoom> _rooms = new Dictionary<string, IGameRoom>();

        // Room 생성 <HTTP API 호출>
        public string CreateRoom(List<int> players)
        {
            string roomId = Guid.NewGuid().ToString().Substring(0, 5);
            IGameRoom room = GameRoomFactory.CreateRoom(roomId, players);
            _rooms[roomId] = room;
            Console.WriteLine($"[GameRoomManager] Room {roomId} created for Players: {string.Join(", ", players)}");
            return roomId;
        }

        // Room 찾기
        public IGameRoom FindRoom(string roomId)
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
