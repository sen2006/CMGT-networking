namespace shared
{
    public class ClientChatMessage : ISerializable
    {
        string text;

        internal ClientChatMessage() { }
        public ClientChatMessage(string text) { this.text = text; }

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
