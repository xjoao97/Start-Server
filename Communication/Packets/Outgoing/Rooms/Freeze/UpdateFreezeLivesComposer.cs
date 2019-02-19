﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Freeze
{
    internal class UpdateFreezeLivesComposer : ServerPacket
    {
        public UpdateFreezeLivesComposer(int UserId, int FreezeLives)
            : base(ServerPacketHeader.UpdateFreezeLivesMessageComposer)
        {
            WriteInteger(UserId);
            WriteInteger(FreezeLives);
        }
    }
}