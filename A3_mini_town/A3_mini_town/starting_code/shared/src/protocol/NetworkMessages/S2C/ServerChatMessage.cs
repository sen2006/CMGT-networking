using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class ServerChatMessage : ISerializable
    {
        string text;
        int senderID;

        public ServerChatMessage() { }
        public ServerChatMessage(string text, int senderID)
        {
            this.text = text;
            this.senderID = senderID;
        }

        public string readText() => text;
        public int getSenderID() => senderID;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(text);
            pPacket.Write(senderID);
        }

        public void Deserialize(Packet pPacket)
        {
            text = pPacket.ReadString();
            senderID = pPacket.ReadInt();
        }
    }
}
