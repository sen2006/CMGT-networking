using System.Net.Sockets;
using System.Net;
using shared;

class TCPServerSample
{
	public static void Main(string[] args)
	{
		TCPServerSample server = new TCPServerSample();
		server.run();
	}

	private TcpListener listener;
	private Dictionary<int, ClientData> clients = new Dictionary<int, ClientData>();
	private Queue<Message> pendingMessages = new Queue<Message>();

	private void run()
	{
		Console.WriteLine("Server started on port 55555");

		listener = new TcpListener(IPAddress.Any, 55555);
		listener.Start();

		while (true)
		{
			processNewClients();
			processExistingClients();
			sendMessages();
			Thread.Sleep(100);
		}
	}

    private void removeClient(ClientData client)
	{
		clients.Remove(client.GetID());
		pendingMessages.Enqueue(new Message(new RemoveAvatarMessage(client.GetID()), clients.Values.ToList()));
	}

	private void processNewClients()
	{
		while (listener.Pending())
		{
			TcpClient acceptedClient = listener.AcceptTcpClient();
			ClientData acceptedClientData = new ClientData(acceptedClient);

			pendingMessages.Enqueue(new Message(new AcceptClientMessage(acceptedClientData.GetAvatar()), acceptedClientData));
            pendingMessages.Enqueue(new Message(new UpdateAvatarMessage(acceptedClientData.GetAvatar()), clients.Values.ToList()));

            clients.Add(acceptedClientData.GetID(), acceptedClientData);
			Console.WriteLine("Accepted new client.");
		}
	}

	private void processExistingClients()
	{
		foreach (ClientData client in clients.Values)
		{
			client.HeartbeatTick();
			if (client.HeartbeatFailed())
			{
				removeClient(client);
			}

			if (client.Available == 0) continue;

			NetworkStream stream = client.GetStream();
            Object readObject = null;

			// a try in case a client send something that could not be read
            try
			{
                readObject = StreamUtil.ReadObject(stream);
			}
			catch (Exception e)
			{
				Console.WriteLine($"error reading client message");
				continue;
			}
            if (readObject is HeartBeatMessage heartBeatMessage)
            {
				client.SendHeartbeat();
            }
            else if (readObject is ClientChatMessage chatMessage)
			{

			}
			else if (readObject is ClientMoveRequest clientMoveRequest)
			{
				clients[client.GetID()].MoveAvatar(clientMoveRequest.GetPosition());
				pendingMessages.Enqueue(new Message(new UpdateAvatarMessage(client.GetAvatar()), clients.Values.ToList()));
			}
		}
	}

    private void sendMessages()
    {
		while(pendingMessages.Count>0)
		{
			Message message = pendingMessages.Dequeue();
			message.Send();
		}
		pendingMessages.Clear();
    }
}

class Message
{
	readonly List<ClientData> recipients = new List<ClientData>();
	readonly ISerializable toSend;

	public Message(ISerializable sendObject, ClientData recipient)
	{
		toSend = sendObject;
		recipients.Add(recipient);
	}

    public Message(ISerializable sendObject, List<ClientData> recipients)
    {
        toSend = sendObject;
        this.recipients.AddRange(recipients);
    }

    public void AddRecipient(ClientData recipient)
	{
        recipients.Add(recipient);
    }

	public void Send()
	{
		WriteObjectToAll(recipients, toSend);
	}

    static void WriteObjectToAll<T>(List<ClientData> clients, T pObject) where T : ISerializable
    {
        foreach (ClientData client in clients)
        {
			if (!clients.Contains(client)) continue;
            StreamUtil.WriteObject(client.GetStream(), pObject);
        }
    }
}

