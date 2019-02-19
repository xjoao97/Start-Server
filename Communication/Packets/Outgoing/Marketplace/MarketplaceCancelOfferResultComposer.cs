﻿namespace Oblivion.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketplaceCancelOfferResultComposer : ServerPacket
    {
        public MarketplaceCancelOfferResultComposer(int OfferId, bool Success)
            : base(ServerPacketHeader.MarketplaceCancelOfferResultMessageComposer)
        {
            WriteInteger(OfferId);
            WriteBoolean(Success);
        }
    }
}