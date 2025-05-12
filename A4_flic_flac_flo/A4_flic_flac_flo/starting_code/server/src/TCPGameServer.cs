using System;
using System.Net.Sockets;
using System.Net;
using shared;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace server {

	/**
	 * Basic TCPGameServer that runs our game.
	 * 
	 * Server is made up out of different rooms that can hold different members.
	 * Each member is identified by a TcpMessageChannel, which can also be used for communication.
	 * In this setup each client is only member of ONE room, but you could change that of course.
	 * 
	 * Each room is responsible for cleaning up faulty clients (since it might involve gameplay, status changes etc).
	 * 
	 * As you can see this setup is limited/lacking:
	 * - only 1 game can be played at a time
	 */
	class TCPGameServer
	{
		static TCPGameServer _instance;

		public static void Main(string[] args)
		{
			TCPGameServer tcpGameServer = new TCPGameServer();
			tcpGameServer.run();
		}

		//we have 3 different rooms at the moment (aka simple but limited)

		private LoginRoom _loginRoom;	//this is the room every new user joins
		private LobbyRoom _lobbyRoom;	//this is the room a user moves to after a successful 'login'
		//private GameRoom _gameRoom;		//this is the room a user moves to when a game is succesfully started
		private List<GameRoom> _gameRooms = new List<GameRoom>();

		//stores additional info for a player
		private Dictionary<TcpMessageChannel, PlayerInfo> _playerInfo = new Dictionary<TcpMessageChannel, PlayerInfo>();
        private List<TcpMessageChannel> _allMembers {get{return _playerInfo.Keys.ToList();}}
        private List<PlayerInfo> _rawPlayerInfo {get{ return _playerInfo.Values.ToList(); }}

        private static Thread heartBeat = new Thread(HeartBeat);

        private TCPGameServer()
		{
            _instance = this;
			//we have only one instance of each room, this is especially limiting for the game room (since this means you can only have one game at a time).
			_loginRoom = new LoginRoom(this);
			_lobbyRoom = new LobbyRoom(this);
			//_gameRoom = new GameRoom(this);
			heartBeat.Start(0);
		}

		private void run()
		{
			Log.LogInfo("Starting server on port 55555", this, ConsoleColor.Gray);

			//start listening for incoming connections (with max 50 in the queue)
			//we allow for a lot of incoming connections, so we can handle them
			//and tell them whether we will accept them or not instead of bluntly declining them
			TcpListener listener = new TcpListener(IPAddress.Any, 55555);
			listener.Start(50);

			while (true)
			{
				//check for new members	
				if (listener.Pending())
				{
					//get the waiting client
					Log.LogInfo("Accepting new client...", this, ConsoleColor.White);
					TcpClient client = listener.AcceptTcpClient();
					//and wrap the client in an easier to use communication channel
					TcpMessageChannel channel = new TcpMessageChannel(client);
					//and add it to the login room for further 'processing'
					_loginRoom.AddMember(channel);
				}

                RemoveEmptyGameRooms();

				//now update every single room
				_loginRoom.Update();
				_lobbyRoom.Update();
				foreach (GameRoom gameRoom in _gameRooms)
					gameRoom.Update();

				Thread.Sleep(100);
			}

		}

        private void RemoveEmptyGameRooms()
        {
			
            for (int i = _gameRooms.Count-1; i>=0; i--)
			{
                GameRoom gameRoom = _gameRooms[i];
                if (gameRoom.IsGameFinished())
                {
					gameRoom.MoveAllMembers(_lobbyRoom);
                    _gameRooms.Remove(gameRoom);
                    Console.WriteLine("removing finished game room");
                }
                else if (gameRoom.GetMembers.Count < 2)
				{
					_gameRooms.Remove(gameRoom);
					Console.WriteLine("removing empty game room");
				}
			}
        }

        //provide access to the different rooms on the server 
        public LoginRoom GetLoginRoom() { return _loginRoom; }
		public LobbyRoom GetLobbyRoom() { return _lobbyRoom; }
		public GameRoom GetGameRoom(int id) { return _gameRooms[id]; }

		/**
		 * Returns a handle to the player info for the given client 
		 * (will create new player info if there was no info for the given client yet)
		 */
		public PlayerInfo GetPlayerInfo (TcpMessageChannel pClient)
		{
			if (!_playerInfo.ContainsKey(pClient))
			{
				_playerInfo[pClient] = new PlayerInfo();
			}

			return _playerInfo[pClient];
		}

		public List<TcpMessageChannel> GetAllMembers()
		{
			return _allMembers;
		}

        public List<PlayerInfo> GetRawPlayerInfo()
        {
            return _rawPlayerInfo;
        }

		/**
		 * Returns a list of all players that match the predicate, e.g. to get a list of 
		 * all players named bob, you would do:
		 *	GetPlayerInfo((playerInfo) => playerInfo.name == "bob");
		 */
		public List<PlayerInfo> GetPlayerInfo(Predicate<PlayerInfo> pPredicate)
		{
			return GetRawPlayerInfo().ToList().FindAll(pPredicate);
		}

		/**
		 * Should be called by a room when a member is closed and removed.
		 */
		public void RemovePlayerInfo (TcpMessageChannel pClient)
		{
			_playerInfo.Remove(pClient);
		}

        public void SendToAll(ASerializable pMessage)
        {
            foreach (TcpMessageChannel member in GetAllMembers())
            {
                member.SendMessage(pMessage);
            }
        }

        public void RemoveAllFaultyMembers()
        {
            _loginRoom.RemoveFaultyMembers();
			_lobbyRoom.RemoveFaultyMembers();
			foreach (GameRoom gameRoom in _gameRooms)
				gameRoom.RemoveFaultyMembers();
        }

        public GameRoom GetGameNewRoom()
        {
			Console.WriteLine("New GameRoom created");
            GameRoom toReturn = new GameRoom(this);
			_gameRooms.Add(toReturn);
			return toReturn;
        }

        private static void HeartBeat(object obj)
        {
            while (true)
			{
				_instance.SendToAll(new EmptyMessage());
				_instance.RemoveAllFaultyMembers();
				Thread.Sleep(10000);
            }
        }
    }


}


