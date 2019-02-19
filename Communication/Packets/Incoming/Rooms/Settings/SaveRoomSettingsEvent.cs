#region

using System;
using System.Collections.Generic;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Navigator;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class SaveRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            var Room = OblivionServer.GetGame().GetRoomManager().LoadRoom(Packet.PopInt());
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            var Name = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Description = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Access = RoomAccessUtility.ToRoomAccess(Packet.PopInt());
            var Password = Packet.PopString();
            var MaxUsers = Packet.PopInt();
            var CategoryId = Packet.PopInt();
            var TagCount = Packet.PopInt();

            var Tags = new List<string>();
            var formattedTags = new StringBuilder();

            for (var i = 0; i < TagCount; i++)
            {
                if (i > 0)
                    formattedTags.Append(",");

                var tag = Packet.PopString();
                Tags.Add(tag);
                formattedTags.Append(tag);
            }

            var TradeSettings = Packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
            var AllowPets = Convert.ToInt32(OblivionServer.BoolToEnum(Packet.PopBoolean()));
            var AllowPetsEat = Convert.ToInt32(OblivionServer.BoolToEnum(Packet.PopBoolean()));
            var AllowWalkthrough = Convert.ToInt32(OblivionServer.BoolToEnum(Packet.PopBoolean())); //override.
            var Hidewall = Convert.ToInt32(OblivionServer.BoolToEnum(Packet.PopBoolean()));
            var WallThickness = Packet.PopInt();
            var FloorThickness = Packet.PopInt();
            var WhoMute = Packet.PopInt(); // mute
            var WhoKick = Packet.PopInt(); // kick
            var WhoBan = Packet.PopInt(); // ban

            var chatMode = Packet.PopInt();
            var chatSize = Packet.PopInt();
            var chatSpeed = Packet.PopInt();
            var chatDistance = Packet.PopInt();
            var extraFlood = Packet.PopInt();

            if (chatMode < 0 || chatMode > 1)
                chatMode = 0;

            if (chatSize < 0 || chatSize > 2)
                chatSize = 0;

            if (chatSpeed < 0 || chatSpeed > 2)
                chatSpeed = 0;

            if (chatDistance < 0)
                chatDistance = 1;

            if (chatDistance > 99)
                chatDistance = 100;

            if (extraFlood < 0 || extraFlood > 2)
                extraFlood = 0;

            if (TradeSettings < 0 || TradeSettings > 2)
                TradeSettings = 0;

            if (WhoMute < 0 || WhoMute > 1)
                WhoMute = 0;

            if (WhoKick < 0 || WhoKick > 1)
                WhoKick = 0;

            if (WhoBan < 0 || WhoBan > 1)
                WhoBan = 0;

            if (WallThickness < -2 || WallThickness > 1)
                WallThickness = 0;

            if (FloorThickness < -2 || FloorThickness > 1)
                FloorThickness = 0;

            if (Name.Length < 1)
                return;

            if (Name.Length > 60)
                Name = Name.Substring(0, 60);

            if (Access == RoomAccess.PASSWORD && Password.Length == 0)
                Access = RoomAccess.OPEN;

            if (MaxUsers < 0)
                MaxUsers = 10;

            if (MaxUsers > 50)
                MaxUsers = 50;

            SearchResultList SearchResultList;
            if (!OblivionServer.GetGame().GetNavigator().TryGetSearchResultList(CategoryId, out SearchResultList))
                CategoryId = 36;

            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY ||
                SearchResultList.RequiredRank > Session.GetHabbo().Rank ||
                Session.GetHabbo().Id != Room.OwnerId && Session.GetHabbo().Rank >= SearchResultList.RequiredRank)
                CategoryId = 36;

            if (TagCount > 2)
                return;

            Room.AllowPets = AllowPets;
            Room.AllowPetsEating = AllowPetsEat;
            Room.RoomBlockingEnabled = AllowWalkthrough;
            Room.Hidewall = Hidewall;

            Room.RoomData.AllowPets = AllowPets;
            Room.RoomData.AllowPetsEating = AllowPetsEat;
            Room.RoomData.RoomBlockingEnabled = AllowWalkthrough;
            Room.RoomData.Hidewall = Hidewall;

            Room.Name = Name;
            Room.Access = Access;
            Room.Description = Description;
            Room.Category = CategoryId;
            Room.Password = Password;

            Room.RoomData.Name = Name;
            Room.RoomData.Access = Access;
            Room.RoomData.Description = Description;
            Room.RoomData.Category = CategoryId;
            Room.RoomData.Password = Password;

            Room.WhoCanBan = WhoBan;
            Room.WhoCanKick = WhoKick;
            Room.WhoCanMute = WhoMute;
            Room.RoomData.WhoCanBan = WhoBan;
            Room.RoomData.WhoCanKick = WhoKick;
            Room.RoomData.WhoCanMute = WhoMute;

            Room.ClearTags();
            Room.AddTagRange(Tags);
            Room.UsersMax = MaxUsers;

            Room.RoomData.Tags.Clear();
            Room.RoomData.Tags.AddRange(Tags);
            Room.RoomData.UsersMax = MaxUsers;

            Room.WallThickness = WallThickness;
            Room.FloorThickness = FloorThickness;
            Room.RoomData.WallThickness = WallThickness;
            Room.RoomData.FloorThickness = FloorThickness;

            Room.ChatMode = chatMode;
            Room.ChatSize = chatSize;
            Room.ChatSpeed = chatSpeed;
            Room.ChatDistance = chatDistance;
            Room.ExtraFlood = extraFlood;

            Room.TradeSettings = TradeSettings;

            Room.RoomData.ChatMode = chatMode;
            Room.RoomData.ChatSize = chatSize;
            Room.RoomData.ChatSpeed = chatSpeed;
            Room.RoomData.ChatDistance = chatDistance;
            Room.RoomData.ExtraFlood = extraFlood;

            Room.RoomData.TradeSettings = TradeSettings;

            var AccessStr = Password.Length > 0 ? "password" : "open";
            switch (Access)
            {
                default:
                case RoomAccess.OPEN:
                    AccessStr = "open";
                    break;

                case RoomAccess.PASSWORD:
                    AccessStr = "password";
                    break;

                case RoomAccess.DOORBELL:
                    AccessStr = "locked";
                    break;

                case RoomAccess.INVISIBLE:
                    AccessStr = "invisible";
                    break;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE rooms SET caption = @caption, description = @description, password = @password, category = " +
                    CategoryId + ", state = '" + AccessStr + "', tags = @tags, users_max = " + MaxUsers +
                    ", allow_pets = '" + AllowPets + "', allow_pets_eat = '" + AllowPetsEat +
                    "', room_blocking_disabled = '" +
                    AllowWalkthrough + "', allow_hidewall = '" + Room.Hidewall + "', floorthick = " +
                    Room.FloorThickness + ", wallthick = " + Room.WallThickness + ", mute_settings='" + Room.WhoCanMute +
                    "', kick_settings='" + Room.WhoCanKick + "',ban_settings='" + Room.WhoCanBan + "', `chat_mode` = '" +
                    Room.ChatMode + "', `chat_size` = '" + Room.ChatSize + "', `chat_speed` = '" + Room.ChatSpeed +
                    "', `chat_extra_flood` = '" + Room.ExtraFlood + "', `chat_hearing_distance` = '" + Room.ChatDistance +
                    "', `trade_settings` = '" + Room.TradeSettings + "' WHERE `id` = '" + Room.RoomId + "' LIMIT 1");
                dbClient.AddParameter("caption", Room.Name);
                dbClient.AddParameter("description", Room.Description);
                dbClient.AddParameter("password", Room.Password);
                dbClient.AddParameter("tags", formattedTags.ToString());
                dbClient.RunQuery();
            }

            Room.GetGameMap().GenerateMaps();

            if (Session.GetHabbo().CurrentRoom == null)
            {
                Session.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
                Session.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));
                Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness,
                    OblivionServer.EnumToBool(Room.Hidewall.ToString())));
            }
            else
            {
                Room.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
                Room.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));
                Room.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness,
                    OblivionServer.EnumToBool(Room.Hidewall.ToString())));
            }

            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModDoorModeSeen", 1);
            OblivionServer.GetGame()
                .GetAchievementManager()
                .ProgressAchievement(Session, "ACH_SelfModWalkthroughSeen", 1);
            OblivionServer.GetGame()
                .GetAchievementManager()
                .ProgressAchievement(Session, "ACH_SelfModChatScrollSpeedSeen", 1);
            OblivionServer.GetGame()
                .GetAchievementManager()
                .ProgressAchievement(Session, "ACH_SelfModChatFloodFilterSeen", 1);
            OblivionServer.GetGame()
                .GetAchievementManager()
                .ProgressAchievement(Session, "ACH_SelfModChatHearRangeSeen", 1);
        }
    }
}