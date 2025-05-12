using shared;
using System;

/**
 * This is where we 'play' a game.
 */
public class GameState : ApplicationStateWithView<GameView>
{
    //just for fun we keep track of how many times a player clicked the board
    //note that in the current application you have no idea whether you are player 1 or 2
    //normally it would be better to maintain this sort of info on the server if it is actually important information
    private int player1MoveCount = 0;
    private int player2MoveCount = 0;

    public override void EnterState()
    {
        base.EnterState();
        
        view.gameBoard.OnCellClicked += _onCellClicked;
        view.ButtonResign.onClick.AddListener(Resign);
    }

    private void _onCellClicked(int pCellIndex)
    {
        MakeMoveRequest makeMoveRequest = new MakeMoveRequest();
        makeMoveRequest.move = pCellIndex;

        fsm.channel.SendMessage(makeMoveRequest);
    }

    public override void ExitState()
    {
        base.ExitState();
        view.gameBoard.OnCellClicked -= _onCellClicked;
    }

    private void Update()
    {
        receiveAndProcessNetworkMessages();
    }

    protected override void handleNetworkMessage(ASerializable pMessage)
    {
        if (pMessage is MakeMoveResult resultMessage) handleMakeMoveResult(resultMessage);
        
        else if (pMessage is UpdateGameState updateMessgae) handleUpdateGameState(updateMessgae);
        
        else if (pMessage is RoomJoinedEvent joinedMessage) handleRoomJoinedEvent(joinedMessage);
    }

    private void handleRoomJoinedEvent(RoomJoinedEvent pMessage)
    {
        if (pMessage.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        }
        if (pMessage.room == RoomJoinedEvent.Room.END_ROOM)
        {
            fsm.ChangeState<EndState>();
        }
    }

    private void handleMakeMoveResult(MakeMoveResult pMakeMoveResult)
    {
        view.gameBoard.SetBoardData(pMakeMoveResult.boardData);
        //some label display
        if (pMakeMoveResult.whoMadeTheMove == 1)
        {
            player1MoveCount++;
        }
        if (pMakeMoveResult.whoMadeTheMove == 2)
        {
            player2MoveCount++;
        }
    }

    private void handleUpdateGameState(UpdateGameState pUpdate)
    {
        view.playerLabel1.text = $"Player 1 ({pUpdate.playerOneName})";
        view.playerLabel2.text = $"Player 2 ({pUpdate.playerTwoName})";

        if (pUpdate.playerAtTurn == 1)
            view.playerLabel1.text += " (at turn)";
        else
            view.playerLabel2.text += " (at turn)";
    }

    private void Resign()
    {
        ResignRequest message = new ResignRequest();
        fsm.channel.SendMessage(message);
    }
}
