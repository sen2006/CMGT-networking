namespace shared
{
    /**
     * Send from SERVER to all CLIENTS to update the state of the game
     */
    public class UpdateGameState : ASerializable
    {
        public string playerOneName = "p1";
        public string playerTwoName = "p2";
        public int playerAtTurn = 0;
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(playerOneName); 
            pPacket.Write(playerTwoName);
            pPacket.Write(playerAtTurn);
        }

        public override void Deserialize(Packet pPacket)
        {
            playerOneName = pPacket.ReadString();
            playerTwoName = pPacket.ReadString();
            playerAtTurn = pPacket.ReadInt();
        }
    }
}
