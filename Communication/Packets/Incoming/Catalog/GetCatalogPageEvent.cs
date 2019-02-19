#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class GetCatalogPageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PageId = Packet.PopInt();
            var Something = Packet.PopInt();
            var CataMode = Packet.PopString();

            CatalogPage Page;
            if (!OblivionServer.GetGame().GetCatalog().TryGetPage(PageId, out Page))
                return;

            if (!Page.Enabled || !Page.Visible || Page.MinimumRank > Session.GetHabbo().Rank)
                return;

            Session.SendMessage(new CatalogPageComposer(Page, CataMode));
        }
    }
}