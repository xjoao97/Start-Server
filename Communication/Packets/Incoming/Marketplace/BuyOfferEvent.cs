#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Marketplace;
using Oblivion.HabboHotel.Catalog.Marketplace;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class BuyOfferEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var OfferId = Packet.PopInt();

            DataRow Row;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `state`,`timestamp`,`total_price`,`extra_data`,`item_id`,`furni_id`,`user_id`,`limited_number`,`limited_stack` FROM `catalog_marketplace_offers` WHERE `offer_id` = @OfferId LIMIT 1");
                dbClient.AddParameter("OfferId", OfferId);
                Row = dbClient.getRow();
            }

            if (Row == null)
            {
                ReloadOffers(Session);
                return;
            }

            if (Convert.ToString(Row["state"]) == "2")
            {
                Session.SendNotification("Opa, esta oferta não é mais válida.");
                ReloadOffers(Session);
                return;
            }

            if (OblivionServer.GetGame().GetCatalog().GetMarketplace().FormatTimestamp() >
                Convert.ToDouble(Row["timestamp"]))
            {
                Session.SendNotification("Que pena, essa oferta expirou.");
                ReloadOffers(Session);
                return;
            }

            ItemData Item = OblivionServer.GetGame().GetItemManager().GetItem(Convert.ToInt32(Row["item_id"]));
            if (Item == null)
            {
                Session.SendNotification("Este item não existe mais aqui.");
                ReloadOffers(Session);
                return;
            }
            if (Convert.ToInt32(Row["user_id"]) == Session.GetHabbo().Id)
            {
                Session.SendNotification(
                    "Para evitar o aumento médio você não pode comprar suas próprias ofertas do mercado.");
                return;
            }

            if (Convert.ToInt32(Row["total_price"]) > Session.GetHabbo().Credits)
            {
                Session.SendNotification("Oops, you do not have enough credits for this.");
                return;
            }

            Session.GetHabbo().Credits -= Convert.ToInt32(Row["total_price"]);
            Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));


            var GiveItem = ItemFactory.CreateSingleItem(Item, Session.GetHabbo(), Convert.ToString(Row["extra_data"]),
                Convert.ToString(Row["extra_data"]), Convert.ToInt32(Row["furni_id"]),
                Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]));
            if (GiveItem != null)
            {
                Session.GetHabbo().GetInventoryComponent().TryAddItem(GiveItem);
                Session.SendMessage(new FurniListNotificationComposer(GiveItem.Id, 1));

                Session.SendMessage(new PurchaseOkComposer());
                Session.SendMessage(new FurniListAddComposer(GiveItem));
                Session.SendMessage(new FurniListUpdateComposer());
            }


            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("UPDATE `catalog_marketplace_offers` SET `state` = '2' WHERE `offer_id` = '" + OfferId +
                                  "' LIMIT 1");

                dbClient.SetQuery("SELECT `id` FROM catalog_marketplace_data WHERE sprite = " + Item.SpriteId +
                                  " LIMIT 1;");
                var Id = dbClient.getInteger();

                if (Id > 0)
                    dbClient.runFastQuery(
                        "UPDATE `catalog_marketplace_data` SET `sold` = `sold` + 1, `avgprice` = (avgprice + " +
                        Convert.ToInt32(Row["total_price"]) + ") WHERE `id` = " + Id + " LIMIT 1;");
                else
                    dbClient.runFastQuery("INSERT INTO `catalog_marketplace_data` (sprite, sold, avgprice) VALUES ('" +
                                      Item.SpriteId + "', '1', '" + Convert.ToInt32(Row["total_price"]) + "')");


                if (OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketAverages.ContainsKey(Item.SpriteId) &&
                    OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketCounts.ContainsKey(Item.SpriteId))
                {
                    var num3 = OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketCounts[Item.SpriteId];
                    var num4 =
                        OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketAverages[Item.SpriteId] +=
                            Convert.ToInt32(Row["total_price"]);

                    OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketAverages.Remove(Item.SpriteId);
                    OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketAverages.Add(Item.SpriteId, num4);
                    OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketCounts.Remove(Item.SpriteId);
                    OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketCounts.Add(Item.SpriteId, num3 + 1);
                }
                else
                {
                    if (
                        !OblivionServer.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketAverages.ContainsKey(Item.SpriteId))
                        OblivionServer.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketAverages.Add(Item.SpriteId, Convert.ToInt32(Row["total_price"]));

                    if (!OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketCounts.ContainsKey(Item.SpriteId))
                        OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketCounts.Add(Item.SpriteId, 1);
                }
            }

            ReloadOffers(Session);
        }


        private static void ReloadOffers(GameClient Session)
        {
            const int MinCost = -1;
            const int MaxCost = -1;
            const string SearchQuery = "";
//            const int FilterMode = 1;


            DataTable table;
            var builder = new StringBuilder();
            builder.Append("WHERE state = '1' AND timestamp >= " +
                           OblivionServer.GetGame().GetCatalog().GetMarketplace().FormatTimestampString());
//            if (MinCost >= 0)
//                builder.Append(" AND total_price > " + MinCost);
//            if (MaxCost >= 0)
//                builder.Append(" AND total_price < " + MaxCost);
//            if (FilterMode == 1)
//            {
            const string str = "ORDER BY asking_price DESC";
//            }
//            else
//            {
//                str = "ORDER BY asking_price ASC";
//            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `offer_id`,`item_type`,`sprite_id`,`total_price`,`limited_number`,`limited_stack` FROM `catalog_marketplace_offers` " +
                    builder + " " + str + " LIMIT 500");
                dbClient.AddParameter("search_query", "%" + SearchQuery + "%");
                if (SearchQuery.Length >= 1)
                    builder.Append(" AND `public_name` LIKE @search_query");
                table = dbClient.getTable();
            }

            OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
                foreach (DataRow row in table.Rows)
                    if (
                        !OblivionServer.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"])))
                    {
                        var item = new MarketOffer(Convert.ToInt32(row["offer_id"]), Convert.ToInt32(row["sprite_id"]),
                            Convert.ToInt32(row["total_price"]), int.Parse(row["item_type"].ToString()),
                            Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]));
                        OblivionServer.GetGame()
                            .GetCatalog()
                            .GetMarketplace()
                            .MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                        OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(item);
                    }

            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            foreach (var item in OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItems)
                if (dictionary.ContainsKey(item.SpriteId))
                {
                    if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice)
                    {
                        dictionary.Remove(item.SpriteId);
                        dictionary.Add(item.SpriteId, item);
                    }

                    var num = dictionary2[item.SpriteId];
                    dictionary2.Remove(item.SpriteId);
                    dictionary2.Add(item.SpriteId, num + 1);
                }
                else
                {
                    dictionary.Add(item.SpriteId, item);
                    dictionary2.Add(item.SpriteId, 1);
                }

            Session.SendMessage(new MarketPlaceOffersComposer(MinCost, MaxCost, dictionary, dictionary2));
        }
    }
}