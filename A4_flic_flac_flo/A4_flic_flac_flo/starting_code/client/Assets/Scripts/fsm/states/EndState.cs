using shared;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Starting state where you can connect to the server.
 */
public class EndState : ApplicationStateWithView<EndView>
{
    private void Update()
    {
        receiveAndProcessNetworkMessages();
    }

    public override void EnterState()
    {
        base.EnterState();
        view.ButtonLobby.onClick.AddListener(ToLobby);
    }

    public override void ExitState ()
    {
        base.ExitState();
        view.ButtonLobby.onClick.RemoveAllListeners();
    }

    protected override void handleNetworkMessage(ASerializable pMessage)
    {
        if (pMessage is RoomJoinedEvent joinMessage) handleRoomJoinedEvent (joinMessage);
        else if (pMessage is UpdateEndState updateMessage) handleStateUpdate(updateMessage);
    }
    

    private void handleRoomJoinedEvent (RoomJoinedEvent pMessage)
    {
        if (pMessage.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        } 
    }

    private void handleStateUpdate(UpdateEndState pMessage)
    {
        view.text.text = $"player {pMessage.winnerName} is the winner";
    }

    private void ToLobby()
    {
        fsm.channel.SendMessage(new ToLobbyRequest());
    }

}

