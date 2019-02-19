#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Catalog.Marketplace;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketPlaceOffersComposer : ServerPacket
    {
        public MarketPlaceOffersComposer(int MinCost, int MaxCost, Dictionary<int, MarketOffer> dictionary,
            Dictionary<int, int> dictionary2)
            : base(ServerPacketHeader.MarketPlaceOffersMessageComposer)
        {
            WriteInteger(dictionary.Count);
            if (dictionary.Count > 0)
                foreach (var pair in dictionary)
                {
                    WriteInteger(pair.Value.OfferID);
                    WriteInteger(1); //State
                    WriteInteger(1);
                    WriteInteger(pair.Value.SpriteId);

                    WriteInteger(256);
                    WriteString("");
                    WriteInteger(pair.Value.LimitedNumber);
                    WriteInteger(pair.Value.LimitedStack);

                    WriteInteger(pair.Value.TotalPrice);
                    WriteInteger(0);
                    WriteInteger(
                        OblivionServer.GetGame().GetCatalog().GetMarketplace().AvgPriceForSprite(pair.Value.SpriteId));
                    WriteInteger(dictionary2[pair.Value.SpriteId]);
                }
            WriteInteger(dictionary.Count); //Item count to show how many were found.
        }
    }
}