﻿namespace Oblivion.Communication.Packets.Outgoing.Inventory.Trading
{
    internal class TradingAcceptComposer : ServerPacket
    {
        public TradingAcceptComposer(int UserId, bool Accept)
            : base(ServerPacketHeader.TradingAcceptMessageComposer)
        {
            WriteInteger(UserId);
            WriteInteger(Accept ? 1 : 0);
        }
    }
}