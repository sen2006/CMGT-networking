namespace shared
{
    public class EmptyMessage : ASerializable
    {

        public override void Serialize(Packet pPacket) { }
        public override void Deserialize(Packet pPacket) { }

    }
}
