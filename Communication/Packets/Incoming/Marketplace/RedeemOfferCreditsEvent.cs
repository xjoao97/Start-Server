#region

using System;
using System.Data;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Marketplace
{
    internal class RedeemOfferCreditsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var CreditsOwed = 0;

            DataTable Table = null;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `asking_price` FROM `catalog_marketplace_offers` WHERE `user_id` = '" +
                                  Session.GetHabbo().Id + "' AND state = '2'");
                Table = dbClient.GetTable();
            }

            if (Table != null)
            {
                foreach (DataRow row in Table.Rows)
                    CreditsOwed += Convert.ToInt32(row["asking_price"]);

                if (CreditsOwed >= 1)
                {
                    Session.GetHabbo().Credits += CreditsOwed;
                    Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                }

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `catalog_marketplace_offers` WHERE `user_id` = '" +
                                      Session.GetHabbo().Id + "' AND `state` = '2'");
                }
            }
        }
    }
}