namespace shared
{
    // the message that gets sent to all clients about a change on a avatar
    // also used for new client joining
    public class UpdateAvatarMessage : ISerializable
    {
        Avatar avatar;
        public UpdateAvatarMessage() { }
        public UpdateAvatarMessage(Avatar avatar) { this.avatar = avatar; }

        public Avatar GetAvatar() => avatar;

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
