using System;
using System.Collections.Generic;
using System.Net.Sockets;

    public class GameRoomManager
    {
        private static GameRoomManager _instance = new GameRoomManager();
        public static GameRoomManager Instance => _instance;

        private Dictionary<string, GameRoom> _rooms = new Dictionary<string, GameRoom>();

        public GameRoom CreateRoom()
        {
            string roomId = Guid.NewGuid().ToString().Substring(0, 5);
            GameRoom room = GameRoomFactory.CreateRoom(roomId);
            _rooms[roomId] = room;
            Console.WriteLine($"[GameRoomManager] Room not exist, -> Room {roomId} created  ");
            return room;
        }

        // Room 찾기
        public GameRoom FindRoom(string roomId)
        {
            return _rooms.TryGetValue(roomId, out var room) ? room : null;
        }
        public GameRoom FindRoom()
        {
            foreach (var room in _rooms.Values)
            {
                if (room.RoomState is RoomState.Waiting )
                {
                    return room;
                }
            }
            return null;
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
