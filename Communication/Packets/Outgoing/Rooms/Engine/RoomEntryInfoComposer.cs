﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class RoomEntryInfoComposer : ServerPacket
    {
        public RoomEntryInfoComposer(int roomID, bool isOwner)
            : base(ServerPacketHeader.RoomEntryInfoMessageComposer)
        {
            WriteInteger(roomID);
            WriteBoolean(isOwner);
        }
    }
}