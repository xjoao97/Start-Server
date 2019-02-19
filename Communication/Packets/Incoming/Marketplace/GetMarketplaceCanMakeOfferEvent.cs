#region

using Oblivion.Communication.Packets.Outgoing.Marketplace;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class GetMarketplaceCanMakeOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ErrorCode = Session.GetHabbo().TradingLockExpiry > 0 ? 6 : 1;

            Session.SendMessage(new MarketplaceCanMakeOfferResultComposer(ErrorCode));
        }
    }
}