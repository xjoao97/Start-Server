#region

using System;
using System.Data;
using System.Threading;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class OpenGiftEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var PresentId = Packet.PopInt();
            var Present = Room.GetRoomItemHandler().GetItem(PresentId);
            if (Present == null)
                return;

            if (Present.UserID != Session.GetHabbo().Id)
                return;

            DataRow Data = null;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `base_id`,`extra_data` FROM `user_presents` WHERE `item_id` = '" + Present.Id +
                                  "' LIMIT 1");
                Data = dbClient.getRow();
            }

            if (Data == null)
            {
                Session.SendNotification(
                    "Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                Room.GetRoomItemHandler().RemoveFurniture(null, Present.Id, false);

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + Present.Id + "' LIMIT 1");
                }

                Session.GetHabbo().GetInventoryComponent().RemoveItem(Present.Id);
                return;
            }

            var PurchaserId = 0;
            if (!int.TryParse(Present.ExtraData.Split(Convert.ToChar(5))[2], out PurchaserId))
            {
                Session.SendNotification(
                    "Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                Room.GetRoomItemHandler().RemoveFurniture(null, Present.Id, false);

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + Present.Id + "' LIMIT 1");
                }
                Session.GetHabbo().GetInventoryComponent().RemoveItem(Present.Id);

                return;
            }

            var Purchaser = OblivionServer.GetGame().GetCacheManager().GenerateUser(PurchaserId);
            if (Purchaser == null)
            {
                Session.SendNotification(
                    "Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                Room.GetRoomItemHandler().RemoveFurniture(null, Present.Id, false);

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + Present.Id + "' LIMIT 1");
                }

                Session.GetHabbo().GetInventoryComponent().RemoveItem(Present.Id);
                return;
            }

            ItemData BaseItem = OblivionServer.GetGame().GetItemManager().GetItem(Convert.ToInt32(Data["base_id"]));
            if (BaseItem == null)
            {
                Session.SendNotification("Oops, it appears that the item within the gift is no longer in the hotel!");
                Room.GetRoomItemHandler().RemoveFurniture(null, Present.Id, false);

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Present.Id + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = '" + Present.Id + "' LIMIT 1");
                }

                Session.GetHabbo().GetInventoryComponent().RemoveItem(Present.Id);
                return;
            }


            Present.MagicRemove = true;
            Room.SendMessage(new ObjectUpdateComposer(Present, Convert.ToInt32(Session.GetHabbo().Id)));

            var thread = new Thread(() => FinishOpenGift(Session, BaseItem, Present, Room, Data));
            thread.Start();
        }

        private static void FinishOpenGift(GameClient Session, ItemData BaseItem, Item Present, Room Room, DataRow Row)
        {
            try
            {
                if (BaseItem == null || Present == null || Room == null || Row == null)
                    return;


                Thread.Sleep(1500);

                var ItemIsInRoom = true;

                Room.GetRoomItemHandler().RemoveFurniture(Session, Present.Id);

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "UPDATE `items` SET `base_item` = @BaseItem, `extra_data` = @edata WHERE `id` = " + Present.Id +
                        " LIMIT 1");
                    dbClient.AddParameter("BaseItem", Row["base_id"]);
                    dbClient.AddParameter("edata", Row["extra_data"]);
                    dbClient.RunQuery();

                    dbClient.RunQuery("DELETE FROM `user_presents` WHERE `item_id` = " + Present.Id + " LIMIT 1");
                }

                Present.BaseItem = Convert.ToInt32(Row["base_id"]);
                Present.ResetBaseItem();
                Present.ExtraData = !string.IsNullOrEmpty(Convert.ToString(Row["extra_data"]))
                    ? Convert.ToString(Row["extra_data"])
                    : "";

                if (Present.Data.Type == 's')
                {
                    if (
                        !Room.GetRoomItemHandler()
                            .SetFloorItem(Session, Present, Present.GetX, Present.GetY, Present.Rotation, true, false,
                                true))
                    {
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = " + Present.Id +
                                              " LIMIT 1");
                        }

                        ItemIsInRoom = false;
                    }
                }
                else
                {
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = " + Present.Id + " LIMIT 1");
                    }

                    ItemIsInRoom = false;
                }

                Session.SendMessage(new OpenGiftComposer(Present.Data, Present.ExtraData, Present, ItemIsInRoom));

                Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            }
            catch
            {
            }
        }
    }
}