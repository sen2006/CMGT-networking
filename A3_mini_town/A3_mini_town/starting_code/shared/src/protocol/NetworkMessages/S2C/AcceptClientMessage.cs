namespace shared
{
    // the message that gets returned to the joining client


    public class AcceptClientMessage : ISerializable
    {
        Avatar avatar;

        internal AcceptClientMessage() { }
        public AcceptClientMessage(Avatar avatar)
        {
            this.avatar = avatar;
        }
        
        public Avatar GetAvatar() { return avatar; }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(avatar);
        }

        public void Deserialize(Packet pPacket)
        {
            avatar = (Avatar)pPacket.ReadObject();
        }
    }
}
