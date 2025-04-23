namespace shared
{
    public class RemoveAvatarMessage : ISerializable
    {
        int id;

        internal RemoveAvatarMessage() { }
        public RemoveAvatarMessage(int id) { 
            this.id = id;
        }

        public int GetID() => id;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(id);
        }

        public void Deserialize(Packet pPacket)
        {
            id = pPacket.ReadInt();
        }
    }
}
