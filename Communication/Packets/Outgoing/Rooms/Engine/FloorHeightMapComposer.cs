﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class FloorHeightMapComposer : ServerPacket
    {
        public FloorHeightMapComposer(string Map, int WallHeight)
            : base(ServerPacketHeader.FloorHeightMapMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(WallHeight);
            WriteString(Map);
        }
    }
}