#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class ApplyDecorationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session, true))
                return;

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(Packet.PopInt());
            if (Item == null)
                return;

            if (Item.GetBaseItem() == null)
                return;

            var DecorationKey = string.Empty;
            switch (Item.GetBaseItem().InteractionType)
            {
                case InteractionType.Floor:
                    DecorationKey = "floor";
                    break;

                case InteractionType.Wallpaper:
                    DecorationKey = "wallpaper";
                    break;

                case InteractionType.Landscape:
                    DecorationKey = "landscape";
                    break;
            }

            switch (DecorationKey)
            {
                case "floor":
                    Room.Floor = Item.ExtraData;
                    Room.RoomData.Floor = Item.ExtraData;

                    OblivionServer.GetGame()
                        .GetQuestManager()
                        .ProgressUserQuest(Session, QuestType.FURNI_DECORATION_FLOOR);
                    OblivionServer.GetGame()
                        .GetAchievementManager()
                        .ProgressAchievement(Session, "ACH_RoomDecoFloor", 1);
                    break;

                case "wallpaper":
                    Room.Wallpaper = Item.ExtraData;
                    Room.RoomData.Wallpaper = Item.ExtraData;

                    OblivionServer.GetGame()
                        .GetQuestManager()
                        .ProgressUserQuest(Session, QuestType.FURNI_DECORATION_WALL);
                    OblivionServer.GetGame()
                        .GetAchievementManager()
                        .ProgressAchievement(Session, "ACH_RoomDecoWallpaper", 1);
                    break;

                case "landscape":
                    Room.Landscape = Item.ExtraData;
                    Room.RoomData.Landscape = Item.ExtraData;

                    OblivionServer.GetGame()
                        .GetAchievementManager()
                        .ProgressAchievement(Session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rooms` SET " + DecorationKey + " = @extradata WHERE `id` = '" + Room.RoomId +
                                  "' LIMIT 1");
                dbClient.AddParameter("extradata", Item.ExtraData);
                dbClient.RunQuery();

                dbClient.RunFastQuery("DELETE FROM items WHERE id=" + Item.Id + " LIMIT 1");
            }

            Session.GetHabbo().GetInventoryComponent().RemoveItem(Item.Id);
            Room.SendMessage(new RoomPropertyComposer(DecorationKey, Item.ExtraData));
        }
    }
}