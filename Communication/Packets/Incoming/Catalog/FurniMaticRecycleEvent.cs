using System;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class FurniMaticRecycleEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null) return;
            if (!Session.GetHabbo().InRoom) return;
            var itemsCount = Packet.PopInt();
            for (var i = 0; i < itemsCount; i++)
            {
                var itemId = Packet.PopInt();
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("DELETE FROM `items` WHERE `id` = '" + itemId + "' AND `user_id` = '" +
                                      Session.GetHabbo().Id + "' LIMIT 1");
                }
                Session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
            }

            var reward = OblivionServer.GetGame().GetFurniMaticRewardsManager().GetRandomReward();
            if (reward == null) return;
            int rewardId;
            const int furniMaticBoxId = 1478;
            var data = OblivionServer.GetGame().GetItemManager().GetItem(furniMaticBoxId);
            var time = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
            var maticData = time + "4" + reward.BaseId + "337200";

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `items` (`base_item`,`user_id`,`extra_data`) VALUES ('" + data.Id +
                                  "', '" + Session.GetHabbo().Id + "', @extra_data)");
                dbClient.AddParameter("extra_data", maticData);
                rewardId = Convert.ToInt32(dbClient.InsertQuery());
                dbClient.RunFastQuery("INSERT INTO `user_presents` (`item_id`,`base_id`,`extra_data`) VALUES ('" +
                                  rewardId + "', '" + reward.BaseId + "', '')");
                dbClient.RunFastQuery("DELETE FROM `items` WHERE `id` = " + rewardId + " LIMIT 1;");
            }

            var GiveItem = ItemFactory.CreateGiftItem(data, Session.GetHabbo(), maticData, maticData, rewardId, 0, 0);
            if (GiveItem != null)
            {
                Session.GetHabbo().GetInventoryComponent().TryAddItem(GiveItem);
                Session.SendMessage(new FurniListNotificationComposer(GiveItem.Id, 1));
                Session.SendMessage(new PurchaseOkComposer());
                Session.SendMessage(new FurniListAddComposer(GiveItem));
                Session.SendMessage(new FurniListUpdateComposer());
            }

            var response = new ServerPacket(ServerPacketHeader.FurniMaticReceiveItem);
            response.WriteInteger(1);
            response.WriteInteger(0); // received item id
            Session.SendMessage(response);
        }
    }
}