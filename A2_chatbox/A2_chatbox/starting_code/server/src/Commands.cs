using shared;
using System.Text;

static class Commands
{
    public static void HandleClientCommands(ClientData client, string message)
    {
        try
        {
            // sprit the command up in individual arguments
            string[] args = message.ToLower().Split(' ');

            // check what command was used
            switch (args[0])
            {
                case "/help": ClientCommands.Help(client, args); break;
                case "/setName":
                case "/sn": ClientCommands.SetName(client, args); break;
                case "/name": ClientCommands.Name(client, args); break;
                case "/wisper":
                case "/w": ClientCommands.Wisper(client, args); break;
                case "/list": ClientCommands.List(client, args); break;
                case "/join": ClientCommands.Join(client, args); break;
                case "/rooms": ClientCommands.Rooms(client, args); break;
                case "/room": ClientCommands.Room(client, args); break;
                case "/listroom": ClientCommands.ListRoom(client, args); break;
                case "/listthisroom": ClientCommands.ListThisRoom(client, args); break;
                case "/forcesetname": ClientCommands.ForceSetName(client, args); break;
                case "/kick": ClientCommands.Kick(client, args); break;
                default: throw new Exception("command not recognized");
            }
        }
        catch (Exception e)
        {
            ServerStreamUtil.SendToClient(client, GetServerMessagePrefix() + "exeption: " + e.Message);
        }
    }

    internal static void HandleServerCommands(string message)
    {
        try
        {
            // sprit the command up in individual arguments
            string[] args = message.ToLower().Split(' ');

            // check what command was used
            switch (args[0])
            {
                case "/help": ServerCommands.Help(args); break;
                case "/list": ServerCommands.List(args); break;
                case "/setname": ServerCommands.SetName(args); break;
                case "/kick": ServerCommands.Kick(args); break;
                case "/admin": ServerCommands.Admin(args); break;
                default: throw new Exception("command not recognized");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("exception: " + e.Message);
        }
    }

    private static class ClientCommands
    {
        internal static void Help(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            string message = GetServerMessagePrefix() + "Commands list:\n" +
                "\n" +
                "/setname [name], /sn [name]\n" +
                "-changes your name to [name]\n\n" +
                "/name\n" +
                "-show what name you chose\n\n" +
                "/wisper [name] {message}, /w [name] {message}\n" +
                "-sends a message only to [name]\n\n" +
                "/list\n" +
                "-shows a list of all connected users\n\n" +
                "/join [roomName]\n" +
                "-joins or creates [roonName]\n\n" +
                "/room\n" +
                "-shows the room you are in\n\n" +
                "/rooms\n" +
                "-shows a list of all active rooms\n\n" +
                "/listroom\n" +
                "-shows a list of all users in your room\n\n" +
                "/listthisroom [roomName]\n" +
                "-shows a list of all users in [roomName]";

            if (client.IsAdmin())
                message += "\n\n" +
                    "Admin commands:\n\n" +
                    "/forcesetname [name] [newName]\n" +
                    "-sets the name of [name] to [newName]\n\n" +
                    "/kick [name]\n" +
                    "-kick [name] from the server";

            ServerStreamUtil.SendToClient(client, message);
        }
        internal static void SetName(ClientData client, string[] args)
        {
            if (args.Length != 2) throw new Exception("Invalid number of arguments");
            string nameChoice = args[1];
            if (TCPServerSample.usedNames.Contains(nameChoice) || TCPServerSample.bannedNames.Contains(nameChoice))
            {
                ServerStreamUtil.SendToClient(client, GetServerMessagePrefix() + "name is taken or unavalible");
            }
            else
            {
                string oldName = client.name;
                client.name = args[1];
                TCPServerSample.usedNames.Remove(oldName);
                TCPServerSample.usedNames.Add(nameChoice);

                Console.WriteLine(" - " + GetServerMessagePrefix() + oldName + " changed name to " + client.name);

                ServerStreamUtil.SendToAllClientsInRoomExcept(TCPServerSample.clients, client.GetRoom(), client, GetServerMessagePrefix() + oldName + " changed name to " + client.name);
                ServerStreamUtil.SendToClient(client, GetServerMessagePrefix() + "changed name to " + client.name);
            }
        }
        internal static void Name(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            ServerStreamUtil.SendToClient(client, GetServerMessagePrefix() + "you are " + client.name);
        }
        internal static void Wisper(ClientData client, string[] args)
        {
            if (args.Length < 3) throw new Exception("Invalid number of arguments");
            foreach (ClientData listClient in TCPServerSample.clients)
            {
                if (listClient.name == args[1])
                {
                    string wisperMessage = "";
                    for (int i = 2; i < args.Length; i++)
                    {
                        wisperMessage += " " + args[i];
                    }
                    ServerStreamUtil.SendToClient(client, DateTime.Now.ToString("(HH:mm)") + "[you] wispered to [" + listClient.name + "]" + wisperMessage);
                    ServerStreamUtil.SendToClient(listClient, DateTime.Now.ToString("(HH:mm)") + "[" + client.name + "] wispered to [you]" + wisperMessage);
                    return;
                }
            }
            throw new Exception("could not find user");
        }
        internal static void List(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            string returnMessage = GetServerMessagePrefix() + "list of all connected users \n";
            foreach (ClientData listClient in TCPServerSample.clients)
            {
                returnMessage += " - " + listClient.name;
                if (TCPServerSample.clients.Last() != listClient) returnMessage += "\n";
            }
            ServerStreamUtil.SendToClient(client, returnMessage);
        }
        internal static void Join(ClientData client, string[] args)
        {
            if (args.Length != 2) throw new Exception("Invalid number of arguments");
            client.SetRoom(args[1]);
            ServerStreamUtil.SendToClient(client, DateTime.Now.ToString("(HH:mm)") + "[server] you connected to room: " + args[1]);
        }
        internal static void Rooms(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            string returnMessage = "list of all rooms \n";
            List<string> rooms = new();
            foreach (ClientData checkClient in TCPServerSample.clients)
            {
                if (!rooms.Contains(checkClient.GetRoom()))
                {
                    if (TCPServerSample.clients.First() != checkClient) returnMessage += "\n";
                    rooms.Add(checkClient.GetRoom());
                    returnMessage += " - " + checkClient.GetRoom();
                }
            }
            ServerStreamUtil.SendToClient(client, GetServerMessagePrefix() + returnMessage);
        }
        internal static void Room(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            ServerStreamUtil.SendToClient(client, GetServerMessagePrefix() + "you are connected to room: " + client.GetRoom());
        }
        internal static void ListRoom(ClientData client, string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            string listRoomReturnMessage = "list of all users in your room \n";
            foreach (ClientData checkClient in TCPServerSample.clients)
            {
                if (checkClient.GetRoom() == client.GetRoom())
                {
                    listRoomReturnMessage += " - " + checkClient.name;
                    if (TCPServerSample.clients.Last() != checkClient) listRoomReturnMessage += "\n";
                }
            }
            ServerStreamUtil.SendToClient(client, listRoomReturnMessage);
        }
        internal static void ListThisRoom(ClientData client, string[] args)
        {
            if (args.Length != 2) throw new Exception("Invalid number of arguments");
            string room = args[1];
            string listRoomReturnMessage = "list of all users in "+room+"\n";
            foreach (ClientData checkClient in TCPServerSample.clients)
            {
                if (checkClient.GetRoom() == room)
                {
                    listRoomReturnMessage += " - " + checkClient.name;
                    if (TCPServerSample.clients.Last() != checkClient) listRoomReturnMessage += "\n";
                }
            }
            ServerStreamUtil.SendToClient(client, listRoomReturnMessage);
        }

        // admin commands
        internal static void ForceSetName(ClientData client, string[] args)
        {
            if (args.Length != 3) throw new Exception("Invalid number of arguments");
            if (!client.IsAdmin()) throw new Exception("You must be an admin to use this");
            string oldName = args[1];
            for (int i = 0; i < TCPServerSample.clients.Count; i++)
            {
                ClientData listClient = TCPServerSample.clients[i];
                if (listClient.name == oldName)
                {
                    listClient.name = args[2];
                    TCPServerSample.usedNames.Remove(oldName);
                    TCPServerSample.usedNames.Add(args[2]);

                    Console.WriteLine(" - " + GetServerMessagePrefix() + client.name +" changed " + oldName + "'s name to " + listClient.name);

                    ServerStreamUtil.SendToAllClientsInRoomExcept(TCPServerSample.clients, listClient.GetRoom(), listClient, GetServerMessagePrefix() + "an admin changed " + oldName + "'s name to " + listClient.name);
                    ServerStreamUtil.SendToClient(listClient, GetServerMessagePrefix() + "an admin changed your name to " + listClient.name);
                    return;
                }
            }
            throw new Exception("could not find user");
        }
        internal static void Kick(ClientData client, string[] args)
        {
            if (args.Length != 2) throw new Exception("Invalid number of arguments");
            if (!client.IsAdmin()) throw new Exception("You must be an admin to use this");
            string kickName = args[1];
            for (int i = 0; i < TCPServerSample.clients.Count; i++)
            {
                ClientData listClient = TCPServerSample.clients[i];
                if (listClient.name == kickName)
                {
                    ServerStreamUtil.SendToClient(listClient, GetServerMessagePrefix() + "You have been kicked by an admin");
                    ServerStreamUtil.SendToClient(client, GetServerMessagePrefix()+ "kicked " + listClient.name);
                    Console.WriteLine($"{client.name} kicked {listClient.name}");
                    TCPServerSample.LateDisconnectClient(listClient);
                    return;
                }
            }
            throw new Exception("could not find user");
        }
    }
    private static class ServerCommands
    {
        internal static void Help(string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            Console.WriteLine("Commands\n" +
                "\n" +
                "/kick [name]\n" +
                "-kicks [name] from the server\n" +
                "\n" +
                "/setname [name] [newName]\n" +
                "-sets the name of [name] to [newName]\n" +
                "\n" +
                "/list\n" +
                "-lists all connected players" +
                "\n" +
                "/admin [add/remove] [name]\n" +
                "-adds or removes [name] as admin");
        }
        internal static void List(string[] args)
        {
            if (args.Length != 1) throw new Exception("Invalid number of arguments");
            string returnMessage = "list of all connected users \n";
            foreach (ClientData listClient in TCPServerSample.clients)
            {
                returnMessage += $" - {listClient.name} room:{listClient.GetRoom()}";
                if (TCPServerSample.clients.Last() != listClient) returnMessage += "\n";
            }
            Console.WriteLine(returnMessage);
        }
        internal static void SetName(string[] args)
        {
            if (args.Length != 3) throw new Exception("Invalid number of arguments");
            string oldName = args[1];
            for (int i = 0; i < TCPServerSample.clients.Count; i++)
            {
                ClientData listClient = TCPServerSample.clients[i];
                if (listClient.name == oldName)
                {
                    listClient.name = args[2];
                    TCPServerSample.usedNames.Remove(oldName);
                    TCPServerSample.usedNames.Add(args[2]);

                    Console.WriteLine("changed name");
                    Console.WriteLine(" - " + GetServerMessagePrefix() + "server changed " + oldName + "'s name to " + listClient.name);

                    ServerStreamUtil.SendToAllClientsInRoomExcept(TCPServerSample.clients, listClient.GetRoom(), listClient, GetServerMessagePrefix() + "server changed " + oldName + "'s name to " + listClient.name);
                    ServerStreamUtil.SendToClient(listClient, GetServerMessagePrefix() + "server changed your name to " + listClient.name);
                    return;
                }
            }
            throw new Exception("could not find user");
        }
        internal static void Kick(string[] args)
        {
            if (args.Length != 2) throw new Exception("Invalid number of arguments");
            string kickName = args[1];
            for (int i = 0; i < TCPServerSample.clients.Count; i++)
            {
                ClientData listClient = TCPServerSample.clients[i];
                if (listClient.name == kickName)
                {
                    ServerStreamUtil.SendToClient(listClient, GetServerMessagePrefix() + "You have been kicked by the server");
                    Console.WriteLine("kicked " + listClient.name);
                    TCPServerSample.LateDisconnectClient(listClient);
                    return;
                }
            }
            throw new Exception("could not find user");
        }
        internal static void Admin(string[] args)
        {
            if (args.Length != 3) throw new Exception("Invalid number of arguments");
            if (args[1] != "add" && args[1] != "remove") throw new Exception("Please use /admin [add/remove] [name]");
            string adminName = args[2];
            for (int i = 0; i < TCPServerSample.clients.Count; i++)
            {
                ClientData listClient = TCPServerSample.clients[i];
                if (listClient.name == adminName)
                {
                    if (args[1] == "add")
                    {
                        ServerStreamUtil.SendToClient(listClient, GetServerMessagePrefix() + "You have been made admin by the server");
                        Console.WriteLine($"made {listClient.name} admin");
                        listClient.SetAdmin(true);
                    } else if (args[1] == "remove")
                    {
                        ServerStreamUtil.SendToClient(listClient, GetServerMessagePrefix() + "You have been removed as admin by the server");
                        Console.WriteLine($"removed {listClient.name} as admin");
                        listClient.SetAdmin(false);
                    }
                    return;
                }
            }
            throw new Exception("could not find user");
        }
    }

    private static string GetServerMessagePrefix() => DateTime.Now.ToString("(HH:mm)[Server] ");
}

