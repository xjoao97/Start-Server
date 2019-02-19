﻿namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class UnknownGroupComposer : ServerPacket
    {
        public UnknownGroupComposer(int GroupId, int HabboId)
            : base(ServerPacketHeader.UnknownGroupMessageComposer)
        {
            WriteInteger(GroupId);
            WriteInteger(HabboId);
        }
    }
}