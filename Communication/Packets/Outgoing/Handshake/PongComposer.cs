﻿namespace Oblivion.Communication.Packets.Outgoing.Handshake
{
    internal class PongComposer : ServerPacket
    {
        public PongComposer()
            : base(ServerPacketHeader.PongMessageComposer)
        {
        }
    }
}