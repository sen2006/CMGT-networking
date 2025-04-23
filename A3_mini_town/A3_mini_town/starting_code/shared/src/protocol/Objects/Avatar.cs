﻿using System;
using System.Numerics;

namespace shared
{
    public class Avatar : ISerializable
    {
        private int id;
        private int skinID;
        private Vector3 position;

        internal Avatar(){}

        public Avatar(int id, int skinID)
        {
            this.id = id;
            this.skinID = skinID;
        }

        public int GetID() => id;
        public int getSkinID() => skinID;
        public void SetPos(Vector3 pos) => position = pos;

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(id);
            pPacket.Write(skinID);
            pPacket.Write(position);
        }

        public void Deserialize(Packet pPacket)
        {
            id = pPacket.ReadInt();
            skinID = pPacket.ReadInt();
            position = pPacket.ReadVec3();
        }
    }
}

