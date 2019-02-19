#region

using System;
using System.Data;
using Oblivion.Communication.Packets.Outgoing.Marketplace;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class GetMarketplaceItemStatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ItemId = Packet.PopInt();
            var SpriteId = Packet.PopInt();

            DataRow Row;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `avgprice` FROM `catalog_marketplace_data` WHERE `sprite` = @SpriteId LIMIT 1");
                dbClient.AddParameter("SpriteId", SpriteId);
                Row = dbClient.getRow();
            }

            Session.SendMessage(new MarketplaceItemStatsComposer(ItemId, SpriteId,
                Row != null ? Convert.ToInt32(Row["avgprice"]) : 0));
        }
    }
}