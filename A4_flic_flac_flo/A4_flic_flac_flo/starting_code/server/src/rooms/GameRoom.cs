using shared;
using System;

namespace server
{
	/**
	 * This room runs a single Game (at a time). 
	 * 
	 * The 'Game' is very simple at the moment:
	 *	- all client moves are broadcasted to all clients
	 *	
	 * The game has no end yet (that is up to you), in other words:
	 * all players that are added to this room, stay in here indefinitely.
	 */
	class GameRoom : Room
	{
		enum State
		{
			PlayerOneTurn,
			PlayerTwoTrurn,
			Finished
		}

		readonly static int[][] winStates =
			[
			[1, 2, 3],
			[4, 5, 6],
			[7, 8, 9],
			[1, 4, 7],
			[2, 5, 8],
			[3, 6, 9],
			[1, 5, 9],
			[3, 5, 7],
			];

		private int wonMember;

		private State gameState = State.PlayerOneTurn;

		public bool IsGameInPlay { get; private set; }

		//wraps the board to play on...
		private TicTacToeBoard _board = new TicTacToeBoard();

		public GameRoom(TCPGameServer pOwner) : base(pOwner)
		{
		}

		public void StartGame (TcpMessageChannel pPlayer1, TcpMessageChannel pPlayer2)
		{
			if (IsGameInPlay) throw new Exception("Programmer error duuuude.");

			IsGameInPlay = true;
			addMember(pPlayer1);
			addMember(pPlayer2);
			updateClientState();

        }

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//notify client he has joined a game room 
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.GAME_ROOM;
			pMember.SendMessage(roomJoinedEvent);
        }

		public override void Update()
		{
			//demo of how we can tell people have left the game...
			int oldMemberCount = memberCount;
			base.Update();
			int newMemberCount = memberCount;

			if (oldMemberCount != newMemberCount)
			{
				Log.LogInfo("People left the game...", this);
			}
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is MakeMoveRequest)
			{
				handleMakeMoveRequest(pMessage as MakeMoveRequest, pSender);
			}
			else if (pMessage is ResignRequest) { HandleResign(pSender); }
		}

		private void handleMakeMoveRequest(MakeMoveRequest pMessage, TcpMessageChannel pSender)
		{
			//we have two players, so index of sender is 0 or 1, which means playerID becomes 1 or 2
			int playerID = indexOfMember(pSender) + 1;
			//make the requested move (0-8) on the board for the player
			if (gameState == State.Finished) return;
			if (!_board.IsAvalibleSpace(pMessage.move)) return;

            if ((gameState == State.PlayerOneTurn && playerID == 1) ||
				(gameState == State.PlayerTwoTrurn && playerID == 2))
			{
				_board.MakeMove(pMessage.move, playerID);

				//and send the result of the boardstate back to all clients
				MakeMoveResult makeMoveResult = new MakeMoveResult();
				makeMoveResult.whoMadeTheMove = playerID;
				makeMoveResult.boardData = _board.GetBoardData();
				sendToAll(makeMoveResult);

				if (CheckGameState(pSender)) return;

				switch (gameState)
				{
					case State.PlayerOneTurn: gameState = State.PlayerTwoTrurn; break;
					case State.PlayerTwoTrurn: gameState = State.PlayerOneTurn; break;
				}
				updateClientState();
            }
		}

		private bool CheckGameState(TcpMessageChannel lastMove)
		{
			foreach (int[] winState in winStates)
			{
				int checkState = _board.GetBoardData().board[winState[0]-1];
				if (checkState == 0) continue;

                if (_board.GetBoardData().board[winState[1]-1] == checkState &&
                    _board.GetBoardData().board[winState[2]-1] == checkState)
				{
                    gameState = State.Finished;
					wonMember = checkState;
					Console.WriteLine(checkState+" won the game");

					MoveAllMembers(_server.GetLobbyRoom());
					_server.GetLobbyRoom().AnnounceWin(lastMove);

                    return true;
                }
            }
			return false;
		}

		private void updateClientState()
		{
			UpdateGameState message = new UpdateGameState();
			message.playerOneName = _server.GetPlayerInfo(GetMembers[0]).GetName();
			message.playerTwoName = _server.GetPlayerInfo(GetMembers[1]).GetName();
			message.playerAtTurn = gameState == State.PlayerOneTurn ? 1 : 2;
			sendToAll(message);
		}

		private void HandleResign(TcpMessageChannel pSender) 
		{
            gameState = State.Finished;
			wonMember = indexOfMember(pSender) == 1 ? 1 : 2;
            Console.WriteLine(wonMember + " won the game by resignation");
			TcpMessageChannel winner = GetMembers[wonMember - 1];
            MoveAllMembers(_server.GetLobbyRoom());
            _server.GetLobbyRoom().AnnounceResign(winner);
        }

		public bool IsGameFinished()
		{
			return gameState == State.Finished;
		}
    }
}
