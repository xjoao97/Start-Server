#region

using Oblivion.Communication.Packets.Outgoing.Marketplace;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class GetOwnOffersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => Session.SendMessage(new MarketPlaceOwnOffersComposer(Session.GetHabbo().Id));
    }
}