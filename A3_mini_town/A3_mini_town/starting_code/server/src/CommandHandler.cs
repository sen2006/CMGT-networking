
using shared;

internal class CommandHandler
{
    internal static void HandleCommand(ClientData client, string command)
    {
        try
        {
            string[] args = command.ToLower().Split(' ');
            switch (args[0])
            {
                case "/wisper":
                case "/w": ClientCommands.Wisper(client, args); break;
                case "/skin": ClientCommands.Skin(client, args); break;
                default: throw new Exception("command not recognized");
            }
        } catch (Exception e)
        {
            Console.WriteLine($"client had error in command ({command})");
            Console.WriteLine(e.Message);
        }
    }



    private static class ClientCommands
    {
        internal static void Wisper(ClientData client, string[] args)
        {
            if (args.Length < 2) throw new Exception("Invalid number of arguments");
            string wisperMessage = "";
            for (int i = 1; i < args.Length; i++)
            {
                wisperMessage += " " + args[i];
            }
            List<ClientData> recievingClients = new List<ClientData>();
            foreach (ClientData recievingClient in TCPServerSample.clients.Keys)
            {
                if ((TCPServerSample.clients[recievingClient].GetPos() - TCPServerSample.clients[client].GetPos()).Length()<=2)
                {
                    recievingClients.Add(recievingClient);
                }
            }
            TCPServerSample.pendingMessages.Enqueue(new Message(new ServerChatMessage(wisperMessage, TCPServerSample.clients[client].GetID()), recievingClients));
        }
        internal static void Skin(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            TCPServerSample.clients[client].setSkinID(new Random().Next(0, 1000));
            TCPServerSample.pendingMessages.Enqueue(new Message(new UpdateAvatarMessage(TCPServerSample.clients[client]), TCPServerSample.clients.Keys.ToList()));
        }

    }
}


