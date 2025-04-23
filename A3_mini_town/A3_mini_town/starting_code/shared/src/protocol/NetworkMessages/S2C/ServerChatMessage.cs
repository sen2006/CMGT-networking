using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class ServerChatMessage : ISerializable
    {
        string text;

        internal ServerChatMessage() { }
        public ServerChatMessage(string text) { this.text = text; }

        public string readText() => text;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(text);
        }

        public void Deserialize(Packet pPacket)
        {
            text = pPacket.ReadString();
        }
    }
}
