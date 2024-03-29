﻿#region

using System;
using System.Collections.Generic;

#endregion

namespace Oblivion.HabboHotel.Catalog.Marketplace
{
    public class MarketplaceManager
    {
        public Dictionary<int, int> MarketAverages = new Dictionary<int, int>();
        public Dictionary<int, int> MarketCounts = new Dictionary<int, int>();
        public List<int> MarketItemKeys = new List<int>();
        public List<MarketOffer> MarketItems = new List<MarketOffer>();

        public int AvgPriceForSprite(int SpriteID)
        {
            var num = 0;
            var num2 = 0;
            if (MarketAverages.ContainsKey(SpriteID) && MarketCounts.ContainsKey(SpriteID))
            {
                if (MarketCounts[SpriteID] > 0)
                    return MarketAverages[SpriteID] / MarketCounts[SpriteID];
                return 0;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = '" + SpriteID +
                                  "' LIMIT 1");
                num = dbClient.GetInteger();

                dbClient.SetQuery("SELECT `sold` FROM `catalog_marketplace_data` WHERE `sprite` = '" + SpriteID +
                                  "' LIMIT 1");
                num2 = dbClient.GetInteger();
            }

            MarketAverages.Add(SpriteID, num);
            MarketCounts.Add(SpriteID, num2);

            if (num2 > 0)
                return Convert.ToInt32(Math.Ceiling((double) (num / num2)));

            return 0;
        }

        public string FormatTimestampString() => FormatTimestamp().ToString().Split(',')[0];

        public double FormatTimestamp() => OblivionServer.GetUnixTimestamp() - 172800.0;

        public int OfferCountForSprite(int SpriteID)
        {
            var dictionary = new Dictionary<int, MarketOffer>();
            var dictionary2 = new Dictionary<int, int>();
            foreach (var item in MarketItems)
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
            if (dictionary2.ContainsKey(SpriteID))
                return dictionary2[SpriteID];
            return 0;
        }

        public int CalculateComissionPrice(float SellingPrice) => Convert.ToInt32(Math.Ceiling(SellingPrice / 100 * 1));
    }
}