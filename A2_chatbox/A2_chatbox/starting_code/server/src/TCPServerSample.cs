using System.Net.Sockets;
using System.Net;
using shared;
using System.Text;

class TCPServerSample
{
    static readonly int _port = 55555;

    internal static List<ClientData> clients = new();
    internal static List<string> usedNames = new();
    internal static string[] bannedNames = { "server", "you" };

    private static List<ClientData> toDisconnect = new();
    
    private static int guestNum = 1;

    private static Thread consoleThread = new Thread(HandleConsoleInput);

    public static void Main(string[] args)
    {
        Console.WriteLine($"Server started on port {_port}");
        
        // start listening for incoming connections
        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        // start CMD thread
        consoleThread.Start();

        while (true)
        {
            ConnectClients(listener);
            HandleActiveClients();
            CleanupClients();

            //Although technically not required it is good to cut the CPU some slack
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// check if an incoming conection is being made and accept it
    /// </summary>
    private static void ConnectClients(TcpListener listener)
    {
        while (listener.Pending())
        {
            try
            {
                ClientData client = new ClientData(listener.AcceptTcpClient(), "guest" + guestNum++);
                clients.Add(client);
                usedNames.Add(client.name);
                IPEndPoint endpoint = client.Get().Client.RemoteEndPoint as IPEndPoint;
                Console.WriteLine($"Accepted new client {endpoint.Address}:{endpoint.Port}.");

                string echoMessage = $"{GetServerMessagePrefix()}You joined the server as {client.name} in general\ntype /help for command info";
                string publicMessage = $"{GetServerMessagePrefix()}{client.name} joined the server";

                Console.WriteLine(" - " + publicMessage);

                ServerStreamUtil.SendToClient(client, echoMessage); // message for sender
                ServerStreamUtil.SendToAllClientsExcept(clients, client, publicMessage); // to all other clients
            }
            catch (Exception e)
            {
                Console.WriteLine("ran into an error connecting new client");
                Console.WriteLine($"* {e.Source} - {e.Message}");
            }
        }
    }

    /// <summary>
    /// handles all incoming trafic
    /// </summary>
    private static void HandleActiveClients()
    {
        foreach (ClientData client in clients)
        {
            if (client.Available == 0) continue;
            OnRecievedMessage(client);
        }
    }

    /// <summary>
    /// check if the connection has been terminated to fully disconnect it
    /// </summary>
    private static void CleanupClients()
    {
        foreach (ClientData client in toDisconnect)
        {
            DisconnectClient(client);
        }
        toDisconnect.Clear();
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            ClientData client = clients[i];
            if (!client.Get().Connected)
            {
                Console.WriteLine(client.name + " has been disconnected , removing");
                DisconnectClient(client);
            }
        }
    }

    private static void OnRecievedMessage(ClientData sendingClient)
    {
        // input the message stream and decode to a string
        NetworkStream stream = sendingClient.GetStream();
        string messageIn = Encoding.UTF8.GetString(StreamUtil.Read(stream));

        // check if the message is a command
        if (messageIn[0] == '/')
        {
            Commands.HandleClientCommands(sendingClient, messageIn);
            return;
        }

        string echoMessage = DateTime.Now.ToString("(HH:mm)") + "[You] " + messageIn;  // return message for sender
        string publicMessage = DateTime.Now.ToString("(HH:mm)[") + sendingClient.name + "] " + messageIn; // to all other clients

        Console.WriteLine($" - {sendingClient.GetRoom()} - {publicMessage}");

        ServerStreamUtil.SendToClient(sendingClient, echoMessage);
        ServerStreamUtil.SendToAllClientsInRoomExcept(clients, sendingClient.GetRoom(), sendingClient, publicMessage);
    }

    /// <summary>
    /// terminate connection and remove from lists
    /// </summary>
    internal static void DisconnectClient(ClientData client)
    {
        try
        {
            clients.Remove(client);
            usedNames.Remove(client.name);
            string message = GetServerMessagePrefix() + client.name + " left the server";
            Console.WriteLine(" - " + message);
            ServerStreamUtil.SendToAllClients(clients, message);
            client.Get().Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("ran into an error disconnecting client: " + client.name);
            Console.WriteLine("* " + e.Source + "-" + e.Message);
        }
    }

    internal static void LateDisconnectClient(ClientData client)
    {
        toDisconnect.Add(client);
    }

    /// <summary>
    /// thread for the CMD console input
    /// </summary>
    private static void HandleConsoleInput()
    {
        while (true)
        {
            string input = Console.ReadLine();
            if (input.Length == 0) continue;
            if (input[0] == '/')
            {
                Commands.HandleServerCommands(input);
                continue;
            }

            string message = DateTime.Now.ToString("(HH:mm)") + "[Server] " + input;

            Console.WriteLine(" - " + message);

            ServerStreamUtil.SendToAllClients(clients, message);

            Thread.Sleep(100);
        }
    }

    private static string GetServerMessagePrefix() => DateTime.Now.ToString("(HH:mm)[Server] ");
}


