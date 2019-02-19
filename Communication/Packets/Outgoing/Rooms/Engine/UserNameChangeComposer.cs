﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserNameChangeComposer : ServerPacket
    {
        public UserNameChangeComposer(int RoomId, int VirtualId, string Username)
            : base(ServerPacketHeader.UserNameChangeMessageComposer)
        {
            WriteInteger(RoomId);
            WriteInteger(VirtualId);
            WriteString(Username);
        }
    }
}