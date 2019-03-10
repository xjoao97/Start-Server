#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class DeleteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().UsersRooms == null)
                return;

            var RoomId = Packet.PopInt();
            if (RoomId == 0)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room))
                return;

            var data = Room.RoomData;
            if (data == null)
                return;

            if (Room.OwnerId != Session.GetHabbo().Id &&
                !Session.GetHabbo().GetPermissions().HasRight("room_delete_any"))
                return;

            var ItemsToRemove = new List<Item>();
            foreach (var Item in Room.GetRoomItemHandler().GetWallAndFloor.ToList().Where(Item => Item != null))
            {
                if (Item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("DELETE FROM `room_items_moodlight` WHERE `item_id` = '" + Item.Id +
                                          "' LIMIT 1");
                    }

                ItemsToRemove.Add(Item);
            }
           
            foreach (var Item in ItemsToRemove)
            {
                var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Item.UserID);
                if (targetClient?.GetHabbo() != null) //Again, do we have an active client?
                {
                    Room.GetRoomItemHandler().RemoveFurniture(targetClient, Item.Id);
                    targetClient.GetHabbo()
                        .GetInventoryComponent()
                        .AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, Item.LimitedNo,
                            Item.LimitedTot);
                    targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else //No, query time.
                {
                    Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + Item.Id + "' LIMIT 1");
                    }
                }
            }

            OblivionServer.GetGame().GetRoomManager().UnloadRoom(Room, true);

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM `user_roomvisits` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunFastQuery("DELETE FROM `rooms` WHERE `id` = '" + RoomId + "' LIMIT 1");
                dbClient.RunFastQuery("DELETE FROM `user_favorites` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunFastQuery("DELETE FROM `items` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunFastQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + RoomId + "'");
                dbClient.RunFastQuery("UPDATE `users` SET `home_room` = '0' WHERE `home_room` = '" + RoomId + "'");
            }

            var removedRoom = (from p in Session.GetHabbo().UsersRooms where p.Id == RoomId select p).SingleOrDefault();
            if (removedRoom != null)
                Session.GetHabbo().UsersRooms.Remove(removedRoom);
        }
    }
}