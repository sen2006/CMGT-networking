using shared;

namespace server
{
    class EndRoom : SimpleRoom
    {
        public EndRoom(TCPGameServer pOwner) : base(pOwner)
        {
        }


        protected override void addMember(TcpMessageChannel pMember)
        {
            base.addMember(pMember);

            RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
            roomJoinedEvent.room = RoomJoinedEvent.Room.END_ROOM;
            pMember.SendMessage(roomJoinedEvent);

            UpdateEndState updateEndState = new UpdateEndState();
            updateEndState.winnerName = _server.GetPlayerInfo(GetMembers[0]).GetName();
            pMember.SendMessage(updateEndState);
            
        }

        protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
        {
            if (pMessage is ToLobbyRequest) handleLobyRequest(pSender);
        }

        private void handleLobyRequest(TcpMessageChannel pSender)
        {
            removeMember(pSender);
            _server.GetLobbyRoom().AddMember(pSender);
        }
    }
}
