using shared;
using System;
using System.Net.Sockets;
using System.Text;


internal static class ServerStreamUtil
{
    public static void SendToClient(ClientData recievingClient, Byte[] bytes)
    {
        try
        {
            StreamUtil.Write(recievingClient.GetStream(), bytes);
        }
        catch (Exception e)
        {
            Console.WriteLine("ran into an error sending message to client:" + recievingClient.name);
            Console.WriteLine("* " + e.Source + "-" + e.Message);
            Console.WriteLine("disconecting client:" + recievingClient.name);
            recievingClient.Get().Close();
        }
    }
    public static void SendToAllClients(List<ClientData> clients, Byte[] bytes)
    {
        foreach (ClientData recievingClient in clients)
        {
            SendToClient(recievingClient, bytes);
        }
    }
    public static void SendToAllClientsExcept(List<ClientData> clients, ClientData exception, Byte[] bytes)
    {
        foreach (ClientData recievingClient in clients)
        {
            if (recievingClient == exception) continue;
            SendToClient(recievingClient, bytes);
        }
    }
    public static void SendToAllClientsInRoom(List<ClientData> clients, string room, Byte[] bytes)
    {
        foreach (ClientData recievingClient in clients)
        {
            if (recievingClient.GetRoom() != room) continue;
            SendToClient(recievingClient, bytes);
        }
    }
    public static void SendToAllClientsInRoomExcept(List<ClientData> clients, string room, ClientData exception, Byte[] bytes)
    {
        foreach (ClientData recievingClient in clients)
        {
            if (recievingClient == exception || recievingClient.GetRoom() != room) continue;
            SendToClient(recievingClient, bytes);
        }
    }

    public static void SendToClient(ClientData recievingClient, string pString) => SendToClient(recievingClient, Encoding.UTF8.GetBytes(pString));
    public static void SendToAllClients(List<ClientData> clients, string pString) => SendToAllClients(clients, Encoding.UTF8.GetBytes(pString));
    public static void SendToAllClientsExcept(List<ClientData> clients, ClientData exception, string pString) => SendToAllClientsExcept(clients, exception, Encoding.UTF8.GetBytes(pString));
    public static void SendToAllClientsInRoom(List<ClientData> clients, string room, string pString) => SendToAllClientsInRoom(clients, room, Encoding.UTF8.GetBytes(pString));
    public static void SendToAllClientsInRoomExcept(List<ClientData> clients, string room, ClientData exception, string pString) => SendToAllClientsInRoomExcept(clients, room, exception, Encoding.UTF8.GetBytes(pString));
}

/// <summary>
/// TcpClient with extra data added
/// </summary>
internal class ClientData
{
    TcpClient client;
    string currentRoom = "general";
    public string name;
    bool admin;

    public ClientData(TcpClient pClient) => client = pClient;
    public ClientData(TcpClient pClient, string pName) : this(pClient) => name = pName;

    public TcpClient Get() => client;

    public string GetRoom() => currentRoom;
    public void SetRoom(string room) { currentRoom = room; }

    public bool IsAdmin() => admin;
    public void SetAdmin(bool state) { admin = state; }

    public int Available => client.Available;
    public NetworkStream GetStream() => client.GetStream();
}
