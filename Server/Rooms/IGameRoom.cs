using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Server;


public interface IGameRoom
{
    string RoomId{ get; }
    bool AddClient(int clientId, ClientSession client);
    void RemoveClient(int clientId);
    void BroadCast(ArraySegment<byte> segment);
    void SendToPlayer(int sessionId, ArraySegment<byte> segment);
    void StartGame();
    void EndGame();
}
