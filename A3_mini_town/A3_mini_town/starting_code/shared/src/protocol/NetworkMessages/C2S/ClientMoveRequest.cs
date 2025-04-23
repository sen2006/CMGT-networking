using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace shared
{
    public class ClientMoveRequest : ISerializable
    {
        Vector3 position;

        public ClientMoveRequest() { }
        public ClientMoveRequest(Vector3 position) { this.position = position; }
        public ClientMoveRequest(float x, float y, float z) { this.position = new Vector3(x,y,z); }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void Deserialize(Packet pPacket)
        {
            position = pPacket.ReadVec3();
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(position);
        }
    }
}
