#region

using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Quests;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class PickupObjectEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
//            Task.Factory.StartNew(() =>
//            {
                if (!Session.GetHabbo().InRoom)
                    return;

                var Room = Session.GetHabbo().CurrentRoom;
                if (Room == null)
                    return;

                var Unknown = Packet.PopInt();
                var ItemId = Packet.PopInt();

                var Item = Room.GetRoomItemHandler().GetItem(ItemId);
                if (Item == null)
                    return;

                if (Item.GetBaseItem().InteractionType == InteractionType.Postit)
                    return;

                var ItemRights = false;
                if (Item.UserID == Session.GetHabbo().Id || Room.CheckRights(Session, false))
                    ItemRights = true;
                else if (Room.Group != null && Room.CheckRights(Session, false, true))
                    //Room has a group, this user has group rights.
                    ItemRights = true;
                else if (Session.GetHabbo().GetPermissions().HasRight("room_item_take"))
                    ItemRights = true;

                if (ItemRights)
                {
                    if (Item.GetBaseItem().InteractionType == InteractionType.Tent ||
                        Item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                        Room.RemoveTent(Item.Id, Item);

                    if (Item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("DELETE FROM `room_items_moodlight` WHERE `item_id` = '" + Item.Id +
                                              "' LIMIT 1");
                        }
                    else if (Item.GetBaseItem().InteractionType == InteractionType.Toner)
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("DELETE FROM `room_items_toner` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                        }


                    if (Item.UserID == Session.GetHabbo().Id)
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);
                        Session.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, Item.LimitedNo,
                                Item.LimitedTot);
                        Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                    }
                    else if (Session.GetHabbo().GetPermissions().HasRight("room_item_take")) //Staff are taking this item
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);
                        Session.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, Item.LimitedNo,
                                Item.LimitedTot);
                        Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
                    }
                    else //Item is being ejected.
                    {
                        var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Item.UserID);
                        if (targetClient?.GetHabbo() != null)
                            //Again, do we have an active client?
                        {
                            Room.GetRoomItemHandler().RemoveFurniture(targetClient, Item.Id);
                            targetClient.GetHabbo()
                                .GetInventoryComponent()
                                .AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true,
                                    Item.LimitedNo,
                                    Item.LimitedTot);
                            targetClient.GetHabbo().GetInventoryComponent().UpdateItems(true);
                        }
                        else //No, query time.
                        {
                            Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + Item.Id +
                                                  "' LIMIT 1");
                            }
                        }
                    }

                    OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_PICK);
                }
//            });
        }
    }
}