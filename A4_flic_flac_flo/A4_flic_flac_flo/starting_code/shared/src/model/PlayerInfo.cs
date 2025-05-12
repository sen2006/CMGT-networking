namespace shared
{
    /**
     * Empty placeholder class for the PlayerInfo object which is being tracked for each client by the server.
     * Add any data you want to store for the player here and make it extend ASerializable.
     */
    public class PlayerInfo : ASerializable
    {
        private string name = "noName";


        public string GetName() { return name; }

        public void SetName(string name) { this.name = name; }


        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(name);
        }
        public override void Deserialize(Packet pPacket)
        {   
            name = pPacket.ReadString();
        }
    }
}
