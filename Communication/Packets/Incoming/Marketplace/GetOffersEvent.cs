#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.Marketplace;
using Oblivion.HabboHotel.Catalog.Marketplace;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class GetOffersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var MinCost = Packet.PopInt();
            var MaxCost = Packet.PopInt();
            var SearchQuery = Packet.PopString();
            var FilterMode = Packet.PopInt();


            DataTable table;
            var builder = new StringBuilder();
            string str;
            builder.Append("WHERE `state` = '1' AND `timestamp` >= " +
                           OblivionServer.GetGame().GetCatalog().GetMarketplace().FormatTimestampString());
            if (MinCost >= 0)
                builder.Append(" AND `total_price` > " + MinCost);


            if (MaxCost >= 0)
                builder.Append(" AND `total_price` < " + MaxCost);

            switch (FilterMode)
            {
                case 1:
                    str = "ORDER BY `asking_price` DESC";
                    break;

                default:
                    str = "ORDER BY `asking_price` ASC";
                    break;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `offer_id`, item_type, sprite_id, total_price, `limited_number`,`limited_stack` FROM catalog_marketplace_offers " +
                    builder + " " + str + " LIMIT 500");
                dbClient.AddParameter("search_query", "%" + SearchQuery + "%");
                if (SearchQuery.Length >= 1)
                    builder.Append(" AND public_name LIKE @search_query");
                table = dbClient.GetTable();
            }

            OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
                foreach (var row in table.Rows.Cast<DataRow>().Where(row => !OblivionServer.GetGame()
                    .GetCatalog()
                    .GetMarketplace()
                    .MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"]))))
                {
                    OblivionServer.GetGame()
                        .GetCatalog()
                        .GetMarketplace()
                        .MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                    OblivionServer.GetGame()
                        .GetCatalog()
                        .GetMarketplace()
                        .MarketItems.Add(new MarketOffer(Convert.ToInt32(row["offer_id"]),
                            Convert.ToInt32(row["sprite_id"]), Convert.ToInt32(row["total_price"]),
                            int.Parse(row["item_type"].ToString()), Convert.ToInt32(row["limited_number"]),
                            Convert.ToInt32(row["limited_stack"])));
                }

            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();

            foreach (var item in OblivionServer.GetGame().GetCatalog().GetMarketplace().MarketItems)
                if (dictionary.ContainsKey(item.SpriteId))
                {
                    if (item.LimitedNumber > 0)
                    {
                        if (!dictionary.ContainsKey(item.OfferID))
                            dictionary.Add(item.OfferID, item);
                        if (!dictionary2.ContainsKey(item.OfferID))
                            dictionary2.Add(item.OfferID, 1);
                    }
                    else
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
                }
                else
                {
                    if (!dictionary.ContainsKey(item.SpriteId))
                        dictionary.Add(item.SpriteId, item);
                    if (!dictionary2.ContainsKey(item.SpriteId))
                        dictionary2.Add(item.SpriteId, 1);
                }

            Session.SendMessage(new MarketPlaceOffersComposer(MinCost, MaxCost, dictionary, dictionary2));
        }
    }
}