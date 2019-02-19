﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class RoomPropertyComposer : ServerPacket
    {
        public RoomPropertyComposer(string name, string val)
            : base(ServerPacketHeader.RoomPropertyMessageComposer)
        {
            WriteString(name);
            WriteString(val);
        }
    }
}