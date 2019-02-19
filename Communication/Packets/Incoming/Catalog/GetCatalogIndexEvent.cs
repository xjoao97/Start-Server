#region

using Oblivion.Communication.Packets.Outgoing.BuildersClub;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class GetCatalogIndexEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            /*int Sub = 0;

            if (Session.GetHabbo().GetSubscriptionManager().HasSubscription)
            {
                Sub = Session.GetHabbo().GetSubscriptionManager().GetSubscription().SubscriptionId;
            }*/

            Session.SendMessage(new CatalogIndexComposer(Session, OblivionServer.GetGame().GetCatalog().GetPages()));
            //, Sub));
            Session.SendMessage(new CatalogItemDiscountComposer());
            Session.SendMessage(new BCBorrowedItemsComposer());
        }
    }
}