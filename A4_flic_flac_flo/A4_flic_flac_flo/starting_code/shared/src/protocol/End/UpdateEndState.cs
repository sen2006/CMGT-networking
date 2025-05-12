namespace shared
{
    public class UpdateEndState : ASerializable
    {
        public string winnerName = "";
        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(winnerName);
        }
        public override void Deserialize(Packet pPacket)
        {
            winnerName = pPacket.ReadString();
        }

    }
}
