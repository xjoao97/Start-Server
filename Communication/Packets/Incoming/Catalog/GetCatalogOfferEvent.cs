#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class GetCatalogOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var OfferId = Packet.PopInt();
            if (!OblivionServer.GetGame().GetCatalog().ItemOffers.Contains(OfferId))
                return;

            var PageId = OblivionServer.GetGame().GetCatalog().ItemOffers[OfferId];
            CatalogPage Page;
            if (!OblivionServer.GetGame().GetCatalog().TryGetPage((int)PageId, out Page))
                return;

            if (!Page.Enabled || !Page.Visible || Page.MinimumRank > Session.GetHabbo().Rank)
                return;

            if (!Page.ItemOffers.Contains(OfferId))
                return;

            var Item = Page.ItemOffers[OfferId];
            if (Item != null)
                Session.SendMessage(new CatalogOfferComposer((CatalogItem)Item));
        }
    }
}