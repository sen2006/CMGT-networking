using System.Collections.Generic;

namespace shared
{
    // the message that gets sent to all clients about a change on a avatar
    // also used for new client joining
    public class UpdateAllAvatarsMessage : ISerializable
    {
        Avatar[] avatars;
        public UpdateAllAvatarsMessage() { }
        public UpdateAllAvatarsMessage(List<Avatar> avatarList) { this.avatars = avatarList.ToArray(); }

        public Avatar[] GetAvatars() => avatars;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(avatars.Length);
            foreach (Avatar avatar in avatars)
                pPacket.Write(avatar);

        }


        public void Deserialize(Packet pPacket)
        {
            int lenth = pPacket.ReadInt();
            avatars = new Avatar[lenth];
            for (int i = 0; i < lenth; i++)
                avatars[i] = (Avatar)pPacket.ReadObject();
        }
    }
}
