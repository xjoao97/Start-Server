#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class PlaceObjectEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            int ItemId;

            var RawData = Packet.PopString();
            var Data = RawData.Split(' ');

            if (!int.TryParse(Data[0], out ItemId))
                return;

            var HasRights = Room.CheckRights(Session, false, true);

            if (!HasRights)
            {
                Session.SendMessage(new RoomNotificationComposer("furni_placement_error", "message",
                    "${room.error.cant_set_not_owner}"));
                return;
            }

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);
            if (Item == null)
                return;

            if (Room.GetRoomItemHandler().GetWallAndFloor.Count() > OblivionStaticGameSettings.RoomFurnitureLimit)
            {
                Session.SendNotification("You cannot have more than " + OblivionStaticGameSettings.RoomFurnitureLimit +
                                         " items in a room!");
                return;
            }
            if (Item.GetBaseItem().ItemName.ToLower().Contains("cf") && Room.OwnerId != Session.GetHabbo().Id &&
                !Session.GetHabbo().GetPermissions().HasRight("room_item_place_exchange_anywhere"))
            {
                Session.SendNotification("You cannot place exchange items in other people's rooms!");
                return;
            }

            //TODO: Make neat.
            switch (Item.GetBaseItem().InteractionType)
            {
                    #region Interaction Types

                case InteractionType.Moodlight:
                {
                    var moodData = Room.MoodlightData;
                    if (moodData != null && Room.GetRoomItemHandler().GetItem(moodData.ItemId) != null)
                    {
                        Session.SendNotification("You can only have one background moodlight per room!");
                        return;
                    }
                    break;
                }
                case InteractionType.Jukebox:
                {
                    if (
                        Room.GetRoomItemHandler()
                            .GetFloor.ToList()
                            .Any(item => item.GetBaseItem().InteractionType == InteractionType.Jukebox))
                    {
                        Session.SendNotification("Você só pode ter uma jukebox por quarto.");
                        return;
                    }
                    break;
                }
                case InteractionType.Toner:
                {
                    var tonerData = Room.TonerData;
                    if (tonerData != null && Room.GetRoomItemHandler().GetItem(tonerData.ItemId) != null)
                    {
                        Session.SendNotification("You can only have one background toner per room!");
                        return;
                    }
                    break;
                }
                case InteractionType.Hopper:
                {
                        if (Room.GetRoomItemHandler().HopperCount > 0)
                    {
                        Session.SendNotification("You can only have one hopper per room!");
                        return;
                    }
                    break;
                }
                case InteractionType.Postit:
                {
                    if (Room.OwnerId != Session.GetHabbo().Id)
                    {
                        OblivionServer.GetGame()
                            .GetAchievementManager()
                            .ProgressAchievement(Session, "ACH_NotesLeft", 1);
                        var owner = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Room.OwnerId);

                        OblivionServer.GetGame()
                            .GetAchievementManager()
                            .ProgressAchievement(owner, "ACH_NotesReceived", 1);
                    }
                    break;
                }

                case InteractionType.Tent:
                case InteractionType.TentSmall:
                {
                    Room.AddTent(Item.Id);
                    break;
                }

                    #endregion
            }
            if (!Item.IsWallItem)
            {
                if (Data.Length < 4)
                    return;

                int X;
                int Y;
                int Rotation;

                if (!int.TryParse(Data[1], out X)) return;
                if (!int.TryParse(Data[2], out Y)) return;
                if (!int.TryParse(Data[3], out Rotation)) return;

                var RoomItem = new Item(Item.Id, Room.RoomId, Item.BaseItem, Item.ExtraData, X, Y, 0, Rotation,
                    Session.GetHabbo().Id, Item.GroupId, Item.LimitedNo, Item.LimitedTot, string.Empty, Room);
                if (Room.GetRoomItemHandler().SetFloorItem(Session, RoomItem, X, Y, Rotation, true, false, true))
                {
                    Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId);

                    if (Session.GetHabbo().Id == Room.OwnerId)
                        OblivionServer.GetGame()
                            .GetAchievementManager()
                            .ProgressAchievement(Session, "ACH_RoomDecoFurniCount", 1);

                    if (RoomItem.IsWired)
                        try
                        {
                            Room.GetWired().LoadWiredBox(RoomItem);
                        }
                        catch
                        {
                            Console.WriteLine(Item.GetBaseItem().InteractionType);
                        }
                }
                else
                {
                    Session.SendMessage(new RoomNotificationComposer("furni_placement_error", "message",
                        "${room.error.cant_set_item}"));
                }
            }
            else if (Item.IsWallItem)
            {
                var CorrectedData = new string[Data.Length - 1];

                for (var i = 1; i < Data.Length; i++)
                    CorrectedData[i - 1] = Data[i];

                var WallPos = string.Empty;

                if (TrySetWallItem(Session.GetHabbo(), Item, CorrectedData, out WallPos))
                    try
                    {
                        var RoomItem = new Item(Item.Id, Room.RoomId, Item.BaseItem, Item.ExtraData, 0, 0, 0, 0,
                            Session.GetHabbo().Id, Item.GroupId, Item.LimitedNo, Item.LimitedTot, WallPos, Room);

                        if (Room.GetRoomItemHandler().SetWallItem(Session, RoomItem))
                        {
                            Session.GetHabbo().GetInventoryComponent().RemoveItem(ItemId);
                            if (Session.GetHabbo().Id == Room.OwnerId)
                                OblivionServer.GetGame()
                                    .GetAchievementManager()
                                    .ProgressAchievement(Session, "ACH_RoomDecoFurniCount", 1);
                        }
                    }
                    catch
                    {
                        Session.SendMessage(new RoomNotificationComposer("furni_placement_error", "message",
                            "${room.error.cant_set_item}"));
                    }
                else
                    Session.SendMessage(new RoomNotificationComposer("furni_placement_error", "message",
                        "${room.error.cant_set_item}"));
            }
        }

        private static bool TrySetWallItem(Habbo Habbo, Item item, string[] data, out string position)
        {
            if (data.Length != 3 || !data[0].StartsWith(":w=") || !data[1].StartsWith("l=") ||
                data[2] != "r" && data[2] != "l")
            {
                position = null;
                return false;
            }

            var wBit = data[0].Substring(3, data[0].Length - 3);
            var lBit = data[1].Substring(2, data[1].Length - 2);

            if (!wBit.Contains(",") || !lBit.Contains(","))
            {
                position = null;
                return false;
            }

            int w1;
            int w2;
            int l1;
            int l2;

            int.TryParse(wBit.Split(',')[0], out w1);
            int.TryParse(wBit.Split(',')[1], out w2);
            int.TryParse(lBit.Split(',')[0], out l1);
            int.TryParse(lBit.Split(',')[1], out l2);
            //
            //if (!Habbo.HasFuse("super_admin") && (w1 < 0 || w2 < 0 || l1 < 0 || l2 < 0 || w1 > 200 || w2 > 200 || l1 > 200 || l2 > 200))
            //{
            //    position = null;
            //    return false;
            //}


            var WallPos = ":w=" + w1 + "," + w2 + " l=" + l1 + "," + l2 + " " + data[2];

            position = WallPositionCheck(WallPos);

            return position != null;
        }

        public static string WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)))
                    return null;
                if (wallPosition.Contains(Convert.ToChar(9)))
                    return null;

                var posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return null;

                var widD = posD[0].Substring(3).Split(',');
                var widthX = int.Parse(widD[0]);
                var widthY = int.Parse(widD[1]);
                if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                    return null;

                var lenD = posD[1].Substring(2).Split(',');
                var lengthX = int.Parse(lenD[0]);
                var lengthY = int.Parse(lenD[1]);
                if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                    return null;

                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return null;
            }
        }
    }
}