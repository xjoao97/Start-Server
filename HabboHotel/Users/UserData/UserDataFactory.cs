#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users.Authenticator;
using Oblivion.HabboHotel.Users.Badges;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Relationships;

#endregion

namespace Oblivion.HabboHotel.Users.UserDataManagement
{
    public class UserDataFactory
    {
        public static UserData GetUserData(string SessionTicket, out byte errorCode)
        {
            int UserId;
            DataRow dUserInfo;
            DataTable dAchievements;
            DataTable dFavouriteRooms;
            DataTable dIgnores;
            DataTable dBadges;
            // DataTable dEffects = null;
            DataTable dFriends;
            DataTable dRequests;
            DataTable dRooms;
            DataTable dQuests;
            DataTable dRelations;
            DataRow userInfo;
            DataTable dBlockedCommands;
            bool disabledEventAlert;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT users.id,users.username,users.auth_ticket,users.rank,users.motto,users.look,users.gender,users.last_online,users.credits,users.activity_points,users.home_room,users.block_newfriends,users.hide_online,users.hide_inroom,users.vip,users.account_created,users.vip_points,users.epoints,users.machine_id,users.nux_user,users.volume,users.chat_preference,users.focus_preference,users.pets_muted,users.bots_muted,users.advertising_report_blocked,users.last_change,users.gotw_points,users.ignore_invites,users.time_muted,users.allow_gifts,users.friend_bar_state,users.disable_forced_effects,users.allow_mimic,users.disabled_alert " +
                    "FROM users WHERE auth_ticket = @sso LIMIT 1");
                dbClient.AddParameter("sso", SessionTicket);
                dUserInfo = dbClient.getRow();

                if (dUserInfo == null || string.IsNullOrEmpty(SessionTicket))
                {
                    errorCode = 1;
                    return null;
                }

                UserId = Convert.ToInt32(dUserInfo["id"]);
                disabledEventAlert = OblivionServer.EnumToBool(dUserInfo["disabled_alert"].ToString());
                if (OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId) != null)
                {
                    errorCode = 2;
                    OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId).Disconnect();
                    return null;
                }

                dbClient.SetQuery("SELECT `group`,`level`,`progress` FROM `user_achievements` WHERE `userid` = '" +
                                  UserId + "'");
                dAchievements = dbClient.getTable();

                dbClient.SetQuery("SELECT room_id FROM user_favorites WHERE `user_id` = '" + UserId + "'");
                dFavouriteRooms = dbClient.getTable();

                dbClient.SetQuery("SELECT ignore_id FROM user_ignores WHERE `user_id` = '" + UserId + "'");
                dIgnores = dbClient.getTable();

                dbClient.SetQuery("SELECT `badge_id`,`badge_slot` FROM user_badges WHERE `user_id` = '" + UserId + "'");
                dBadges = dbClient.getTable();

                //    dbClient.SetQuery(
                //       "SELECT `effect_id`,`total_duration`,`is_activated`,`activated_stamp` FROM user_effects WHERE `user_id` = '" +
                //      UserId + "'");
                // dEffects = dbClient.getTable();

                dbClient.SetQuery(
                    "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
                    "FROM users " +
                    "JOIN messenger_friendships " +
                    "ON users.id = messenger_friendships.user_one_id " +
                    "WHERE messenger_friendships.user_two_id = " + UserId + " " +
                    "UNION ALL " +
                    "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
                    "FROM users " +
                    "JOIN messenger_friendships " +
                    "ON users.id = messenger_friendships.user_two_id " +
                    "WHERE messenger_friendships.user_one_id = " + UserId);
                dFriends = dbClient.getTable();

                dbClient.SetQuery(
                    "SELECT messenger_requests.from_id,messenger_requests.to_id,users.username FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = " +
                    UserId);
                dRequests = dbClient.getTable();

                dbClient.SetQuery("SELECT * FROM rooms WHERE `owner` = '" + UserId + "' LIMIT 150");
                dRooms = dbClient.getTable();

                dbClient.SetQuery("SELECT `quest_id`,`progress` FROM user_quests WHERE `user_id` = '" + UserId + "'");
                dQuests = dbClient.getTable();

                dbClient.SetQuery(
                    "SELECT `id`,`user_id`,`target`,`type` FROM `user_relationships` WHERE `user_id` = '" + UserId + "'");
                dRelations = dbClient.getTable();

                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + UserId + "' LIMIT 1");
                userInfo = dbClient.getRow();
                if (userInfo == null)
                {
                    dbClient.runFastQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + UserId + "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + UserId + "' LIMIT 1");
                    userInfo = dbClient.getRow();
                }


                dbClient.SetQuery("SELECT command_name FROM user_blockcmd WHERE user_id = '" + UserId + "'");
                dBlockedCommands = dbClient.getTable();

                dbClient.runFastQuery("UPDATE `users` SET `online` = '1', `auth_ticket` = '' WHERE `id` = '" + UserId + "'");
                //                dbClient.RunQuery("DELETE FROM `user_auth_ticket` WHERE `user_id` = '" + UserId + "'");
            }

            var Achievements = new ConcurrentDictionary<string, UserAchievement>();
            foreach (DataRow dRow in dAchievements.Rows)
                Achievements.TryAdd(Convert.ToString(dRow["group"]),
                    new UserAchievement(Convert.ToString(dRow["group"]), Convert.ToInt32(dRow["level"]),
                        Convert.ToInt32(dRow["progress"])));

            var favouritedRooms =
                (from DataRow dRow in dFavouriteRooms.Rows select Convert.ToInt32(dRow["room_id"])).ToList();

            var ignores = (from DataRow dRow in dIgnores.Rows select Convert.ToInt32(dRow["ignore_id"])).ToList();

            var badges = (from DataRow dRow in dBadges.Rows
                select new Badge(Convert.ToString(dRow["badge_id"]), Convert.ToInt32(dRow["badge_slot"]))).ToList();

            var friends = new Dictionary<int, MessengerBuddy>();
            foreach (DataRow dRow in dFriends.Rows)
            {
                var friendID = Convert.ToInt32(dRow["id"]);
                var friendName = Convert.ToString(dRow["username"]);
                var friendLook = Convert.ToString(dRow["look"]);
                var friendMotto = Convert.ToString(dRow["motto"]);
                var friendLastOnline = Convert.ToInt32(dRow["last_online"]);
                var friendHideOnline = OblivionServer.EnumToBool(dRow["hide_online"].ToString());
                var friendHideRoom = OblivionServer.EnumToBool(dRow["hide_inroom"].ToString());

                if (friendID == UserId)
                    continue;

                if (!friends.ContainsKey(friendID))
                    friends.Add(friendID,
                        new MessengerBuddy(friendID, friendName, friendLook, friendMotto, friendLastOnline,
                            friendHideOnline, friendHideRoom));
            }

            var requests = new Dictionary<int, MessengerRequest>();
            foreach (DataRow dRow in dRequests.Rows)
            {
                var receiverID = Convert.ToInt32(dRow["from_id"]);
                var senderID = Convert.ToInt32(dRow["to_id"]);

                var requestUsername = Convert.ToString(dRow["username"]);

                if (receiverID != UserId)
                {
                    if (!requests.ContainsKey(receiverID))
                        requests.Add(receiverID, new MessengerRequest(UserId, receiverID, requestUsername));
                }
                else
                {
                    if (!requests.ContainsKey(senderID))
                        requests.Add(senderID, new MessengerRequest(UserId, senderID, requestUsername));
                }
            }

            var rooms = (from DataRow dRow in dRooms.Rows
                    select OblivionServer.GetGame().GetRoomManager().FetchRoomData(Convert.ToInt32(dRow["id"]), dRow))
                .ToList();

            var quests = new Dictionary<int, int>();
            foreach (DataRow dRow in dQuests.Rows)
            {
                var questId = Convert.ToInt32(dRow["quest_id"]);

                if (quests.ContainsKey(questId))
                    quests.Remove(questId);

                quests.Add(questId, Convert.ToInt32(dRow["progress"]));
            }

            var Relationships =
                dRelations.Rows.Cast<DataRow>()
                    .Where(Row => friends.ContainsKey(Convert.ToInt32(Row[2])))
                    .ToDictionary(Row => Convert.ToInt32(Row[2]),
                        Row =>
                            new Relationship(Convert.ToInt32(Row[0]), Convert.ToInt32(Row[2]),
                                Convert.ToInt32(Row[3].ToString())));

            var blockedCommands = (from DataRow r in dBlockedCommands.Rows select r["command_name"].ToString()).ToList();

            var user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);

            errorCode = 0;

            if (user.Rank >= 6)
                user.CustomBubbleId = 23;

            return new UserData(UserId, Achievements, favouritedRooms, ignores, badges, friends, requests, rooms, quests,
                user, Relationships, blockedCommands, disabledEventAlert);
        }

        public static UserData GetUserData(int UserId)
        {
            DataRow dUserInfo;
            DataRow UserInfo;
            DataTable dRelations;
            // DataTable dGroups = null;
            DataTable dBlockedCommands;

            bool disabledEventAlert;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`epoints`,`machine_id`,`volume`,`chat_preference`, `focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`disabled_alert`,`nux_user` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", UserId);
                dUserInfo = dbClient.getRow();

                OblivionServer.GetGame().GetClientManager().LogClonesOut(Convert.ToInt32(UserId));

                if (dUserInfo == null)
                    return null;

                if (OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId) != null)
                    return null;

                disabledEventAlert = OblivionServer.EnumToBool(dUserInfo["disabled_alert"].ToString());


                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + UserId + "' LIMIT 1");
                UserInfo = dbClient.getRow();
                if (UserInfo == null)
                {
                    dbClient.runFastQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + UserId + "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + UserId + "' LIMIT 1");
                    UserInfo = dbClient.getRow();
                }


                dbClient.SetQuery("SELECT command_name FROM user_blockcmd WHERE user_id=@id");
                dbClient.AddParameter("id", UserId);
                dBlockedCommands = dbClient.getTable();

                // dbClient.SetQuery("SELECT group_id,rank FROM group_memberships WHERE user_id=@id");
                // dbClient.AddParameter("id", UserId);
                // dGroups = dbClient.getTable();

                dbClient.SetQuery("SELECT `id`,`target`,`type` FROM user_relationships WHERE user_id=@id");
                dbClient.AddParameter("id", UserId);
                dRelations = dbClient.getTable();
            }

            var achievements = new ConcurrentDictionary<string, UserAchievement>();
            var favouritedRooms = new List<int>();
            var ignores = new List<int>();
            var badges = new List<Badge>();
            var friends = new Dictionary<int, MessengerBuddy>();
            var friendRequests = new Dictionary<int, MessengerRequest>();
            var rooms = new List<RoomData>();
            var quests = new Dictionary<int, int>();
            var blockedCommands = (from DataRow r in dBlockedCommands.Rows select r["command_name"].ToString()).ToList();
            var relationships = new Dictionary<int, Relationship>();
            foreach (
                var Row in
                dRelations.Rows.Cast<DataRow>().Where(Row => !relationships.ContainsKey(Convert.ToInt32(Row["id"]))))
                relationships.Add(Convert.ToInt32(Row["target"]),
                    new Relationship(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["target"]),
                        Convert.ToInt32(Row["type"].ToString())));

            var user = HabboFactory.GenerateHabbo(dUserInfo, UserInfo);
            return new UserData(UserId, achievements, favouritedRooms, ignores, badges, friends, friendRequests, rooms,
                quests, user, relationships, blockedCommands, disabledEventAlert);
        }
    }
}