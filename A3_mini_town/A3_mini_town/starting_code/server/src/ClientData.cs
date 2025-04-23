using shared;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

class ClientData
{
    private static int nextID = 0;
    private static readonly int heartbeatAmount = 20;

    int heartbeat;

    readonly TcpClient client;
    Avatar avatar;    


    public ClientData(TcpClient pClient)
    {
        heartbeat = heartbeatAmount;
        client = pClient;
        avatar = new Avatar(nextID++, new Random().Next(0, 1000));
    }

    public TcpClient GetClient() => client;
    public int GetID() => avatar.GetID();
    public int Available => client.Available;
    public NetworkStream GetStream() => client.GetStream();
    public Avatar GetAvatar() => avatar;
    public void MoveAvatar(Vector3 pos) => avatar.SetPos(pos);
    public void SendHeartbeat() => heartbeat= heartbeatAmount;
    public void HeartbeatTick() => heartbeat--;
    public bool HeartbeatFailed() => heartbeat < 0;
}

