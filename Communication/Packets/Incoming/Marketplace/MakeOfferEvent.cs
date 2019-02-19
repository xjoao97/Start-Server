#region

using Oblivion.Communication.Packets.Outgoing.Marketplace;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class MakeOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var SellingPrice = Packet.PopInt();
            var ComissionPrice = Packet.PopInt();
            var ItemId = Packet.PopInt();

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);
            if (Item == null)
            {
                Session.SendMessage(new MarketplaceMakeOfferResultComposer(0));
                return;
            }

            if (SellingPrice > 70000000 || SellingPrice == 0)
            {
                Session.SendMessage(new MarketplaceMakeOfferResultComposer(0));
                return;
            }

            var Comission = OblivionServer.GetGame().GetCatalog().GetMarketplace().CalculateComissionPrice(SellingPrice);
            var TotalPrice = SellingPrice + Comission;
            var ItemType = 1;
            if (Item.GetBaseItem().Type == 'i')
                ItemType++;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `catalog_marketplace_offers` (`furni_id`,`item_id`,`user_id`,`asking_price`,total_price,public_name,sprite_id,item_type,timestamp,extra_data,limited_number,limited_stack) VALUES ('" +
                    ItemId + "','" + Item.BaseItem + "','" + Session.GetHabbo().Id + "','" + SellingPrice + "','" +
                    TotalPrice + "',@public_name,'" + Item.GetBaseItem().SpriteId + "','" + ItemType + "','" +
                    OblivionServer.GetUnixTimestamp() + "',@extra_data, '" + Item.LimitedNo + "', '" + Item.LimitedTot +
                    "')");
                dbClient.AddParameter("public_name", Item.GetBaseItem().PublicName);
                dbClient.AddParameter("extra_data", Item.ExtraData);
                dbClient.RunQuery();

                dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + ItemId + "' AND `user_id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
            }

            Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId);
            Session.SendMessage(new MarketplaceMakeOfferResultComposer(1));
        }
    }
}