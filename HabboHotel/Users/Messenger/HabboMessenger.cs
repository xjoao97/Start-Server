#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Quests;
using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Users.Messenger
{
    public class HabboMessenger
    {
        private readonly int _userId;

        private Dictionary<int, MessengerBuddy> _friends;
        private Dictionary<int, MessengerRequest> _requests;
        public bool AppearOffline;

        public HabboMessenger(int UserId)
        {
            _userId = UserId;

            _requests = new Dictionary<int, MessengerRequest>();
            _friends = new Dictionary<int, MessengerBuddy>();
        }


        public void Init(Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests)
        {
            _requests = new Dictionary<int, MessengerRequest>(requests);
            _friends = new Dictionary<int, MessengerBuddy>(friends);
        }

        public bool TryGetRequest(int senderID, out MessengerRequest Request)
            => _requests.TryGetValue(senderID, out Request);

        public bool TryGetFriend(int UserId, out MessengerBuddy Buddy) => _friends.TryGetValue(UserId, out Buddy);

        public void ProcessOfflineMessages()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `messenger_offline_messages` WHERE `to_id` = @id;");
                dbClient.AddParameter("id", _userId);
                var GetMessages = dbClient.getTable();

                if (GetMessages == null) return;
                var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(_userId);
                if (Client == null)
                    return;

                foreach (DataRow Row in GetMessages.Rows)
                    Client.SendMessage(new NewConsoleMessageComposer(Convert.ToInt32(Row["from_id"]),
                        Convert.ToString(Row["message"]),
                        (int) (UnixTimestamp.GetNow() - Convert.ToInt32(Row["timestamp"]))));

                dbClient.SetQuery("DELETE FROM `messenger_offline_messages` WHERE `to_id` = @id");
                dbClient.AddParameter("id", _userId);
                dbClient.RunQuery();
            }
        }

        public void Destroy()
        {
            var onlineUsers = OblivionServer.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (
                var client in
                onlineUsers.Where(client => client.GetHabbo() != null && client.GetHabbo().GetMessenger() != null))
                client.GetHabbo().GetMessenger().UpdateFriend(_userId, null, true);
        }

        public void OnStatusChanged(bool notification)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null ||
                GetClient().GetHabbo().GetMessenger() == null)
                return;

            if (_friends == null)
                return;

            var onlineUsers = OblivionServer.GetGame().GetClientManager().GetClientsById(_friends.Keys);
            if (!onlineUsers.Any())
                return;

            foreach (var client in onlineUsers.ToList())
                try
                {
                    if (client?.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                        continue;

                    client.GetHabbo().GetMessenger().UpdateFriend(_userId, client, true);

                    if (client.GetHabbo() == null)
                        continue;

                    UpdateFriend(client.GetHabbo().Id, client, notification);
                }
                catch
                {
                }
        }

        public void UpdateFriend(int userid, GameClient client, bool notification)
        {
            if (!_friends.ContainsKey(userid)) return;
            _friends[userid].UpdateUser(client);

            if (!notification) return;
            var Userclient = GetClient();
            Userclient?.SendMessage(SerializeUpdate(_friends[userid]));
        }

        public void HandleAllRequests()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_requests WHERE from_id = " + _userId + " OR to_id = " + _userId);
            }

            ClearRequests();
        }

        public void HandleRequest(int sender)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_requests WHERE (from_id = " + _userId + " AND to_id = " +
                                  sender + ") OR (to_id = " + _userId + " AND from_id = " + sender + ")");
            }

            _requests.Remove(sender);
        }

        public void CreateFriendship(int friendID)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("REPLACE INTO messenger_friendships (user_one_id,user_two_id) VALUES (" + _userId +
                                  "," + friendID + ")");
            }

            OnNewFriendship(friendID);

            var User = OblivionServer.GetGame().GetClientManager().GetClientByUserID(friendID);

            if (User?.GetHabbo().GetMessenger() != null)
                User.GetHabbo().GetMessenger().OnNewFriendship(_userId);

            if (User != null)
                OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(User, "ACH_FriendListSize", 1);

            var thisUser = OblivionServer.GetGame().GetClientManager().GetClientByUserID(_userId);
            if (thisUser != null)
                OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(thisUser, "ACH_FriendListSize", 1);
        }

        public void DestroyFriendship(int friendID)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_friendships WHERE (user_one_id = " + _userId +
                                  " AND user_two_id = " + friendID + ") OR (user_two_id = " + _userId +
                                  " AND user_one_id = " + friendID + ")");
            }

            OnDestroyFriendship(friendID);

            var User = OblivionServer.GetGame().GetClientManager().GetClientByUserID(friendID);

            if (User?.GetHabbo().GetMessenger() != null)
                User.GetHabbo().GetMessenger().OnDestroyFriendship(_userId);
        }

        public void OnNewFriendship(int friendID)
        {
            var friend = OblivionServer.GetGame().GetClientManager().GetClientByUserID(friendID);

            MessengerBuddy newFriend;
            if (friend?.GetHabbo() == null)
            {
                DataRow dRow;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "SELECT id,username,motto,look,last_online,hide_inroom,hide_online FROM users WHERE `id` = @friendid LIMIT 1");
                    dbClient.AddParameter("friendid", friendID);
                    dRow = dbClient.getRow();
                }

                newFriend = new MessengerBuddy(friendID, Convert.ToString(dRow["username"]),
                    Convert.ToString(dRow["look"]), Convert.ToString(dRow["motto"]),
                    Convert.ToInt32(dRow["last_online"]),
                    OblivionServer.EnumToBool(dRow["hide_online"].ToString()),
                    OblivionServer.EnumToBool(dRow["hide_inroom"].ToString()));
            }
            else
            {
                var user = friend.GetHabbo();


                newFriend = new MessengerBuddy(friendID, user.Username, user.Look, user.Motto, 0, user.AppearOffline,
                    user.AllowPublicRoomStatus);
                newFriend.UpdateUser(friend);
            }

            if (!_friends.ContainsKey(friendID))
                _friends.Add(friendID, newFriend);

            GetClient().SendMessage(SerializeUpdate(newFriend));
        }

        public bool RequestExists(int requestID)
        {
            if (_requests.ContainsKey(requestID))
                return true;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT user_one_id FROM messenger_friendships WHERE user_one_id = @myID AND user_two_id = @friendID");
                dbClient.AddParameter("myID", Convert.ToInt32(_userId));
                dbClient.AddParameter("friendID", Convert.ToInt32(requestID));
                return dbClient.findsResult();
            }
        }

        public bool FriendshipExists(int friendID) => _friends.ContainsKey(friendID);

        public void OnDestroyFriendship(int Friend)
        {
            if (_friends.ContainsKey(Friend))
                _friends.Remove(Friend);

            GetClient().SendMessage(new FriendListUpdateComposer(Friend));
        }

        public bool RequestBuddy(string UserQuery)
        {
            int userID;
            bool hasFQDisabled;

            var client = OblivionServer.GetGame().GetClientManager().GetClientByUsername(UserQuery);
            if (client == null)
            {
                DataRow Row;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id`,`block_newfriends` FROM `users` WHERE `username` = @query LIMIT 1");
                    dbClient.AddParameter("query", UserQuery.ToLower());
                    Row = dbClient.getRow();
                }

                if (Row == null)
                    return false;

                userID = Convert.ToInt32(Row["id"]);
                hasFQDisabled = OblivionServer.EnumToBool(Row["block_newfriends"].ToString());
            }
            else
            {
                userID = client.GetHabbo().Id;
                hasFQDisabled = client.GetHabbo().AllowFriendRequests;
            }

            if (hasFQDisabled)
            {
                GetClient().SendMessage(new MessengerErrorComposer(39, 3));
                return false;
            }

            var ToId = userID;
            if (RequestExists(ToId))
                return true;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("REPLACE INTO `messenger_requests` (`from_id`,`to_id`) VALUES ('" + _userId + "','" +
                                  ToId + "')");
            }

            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(GetClient(), QuestType.ADD_FRIENDS);

            var ToUser = OblivionServer.GetGame().GetClientManager().GetClientByUserID(ToId);
            if (ToUser?.GetHabbo() == null)
                return true;

            var Request = new MessengerRequest(ToId, _userId,
                OblivionServer.GetGame().GetClientManager().GetNameById(_userId));

            ToUser.GetHabbo().GetMessenger().OnNewRequest(_userId);

            var ThisUser = OblivionServer.GetGame().GetCacheManager().GenerateUser(_userId);

            if (ThisUser != null)
                ToUser.SendMessage(new NewBuddyRequestComposer(ThisUser));

            _requests.Add(ToId, Request);
            return true;
        }

        public void OnNewRequest(int friendID)
        {
            if (!_requests.ContainsKey(friendID))
                _requests.Add(friendID,
                    new MessengerRequest(_userId, friendID,
                        OblivionServer.GetGame().GetClientManager().GetNameById(friendID)));
        }

        public void SendGroupMessage(string Message, Group group)
        {
            if (GetClient()?.GetHabbo() == null)
                return;

            if (GetClient().GetHabbo().MessengerSpamCount >= 12)
            {
                GetClient().GetHabbo().MessengerSpamTime = OblivionServer.GetUnixTimestamp() + 60;
                GetClient().GetHabbo().MessengerSpamCount = 0;
                GetClient()
                    .SendNotification(
                        "You cannot send a message, you have flooded the console.\n\nYou can send a message in 60 seconds.");
                return;
            }
            if (GetClient().GetHabbo().MessengerSpamTime > OblivionServer.GetUnixTimestamp())
            {
                var Time = GetClient().GetHabbo().MessengerSpamTime - OblivionServer.GetUnixTimestamp();
                GetClient()
                    .SendNotification(
                        "You cannot send a message, you have flooded the console.\n\nYou can send a message in " + Time +
                        " seconds.");
                return;
            }
            GetClient().GetHabbo().MessengerSpamCount++;
            

            if (GetClient().GetHabbo().TimeMuted > 0)
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.YOUR_MUTED, group.Id));
                return;
            }

            if (string.IsNullOrEmpty(Message))
                return;
            var members = group.GetAllMembers.ToList();
            var Sender = GetClient().GetHabbo();
            foreach (var member in members)
            {
                var client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(member);
                if (client?.GetHabbo() == null || client.GetHabbo().Id == Sender.Id || !group.ChatUsers.Contains(client.GetHabbo().Id))
                    continue;

                client.SendMessage(new NewConsoleMessageComposer(group.Id, Message, 0, Sender.Id, Sender.Username, Sender.Look));
            }
            // Client.SendMessage(new NewConsoleMessageComposer(_userId, Message));
        }

        public void SendInstantMessage(int ToId, string Message)
        {
            if (ToId == 0)
                return;

            if (GetClient()?.GetHabbo() == null)
                return;

            if (!FriendshipExists(ToId))
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.YOUR_NOT_FRIENDS, ToId));
                return;
            }


            if (GetClient().GetHabbo().MessengerSpamCount >= 12)
            {
                GetClient().GetHabbo().MessengerSpamTime = OblivionServer.GetUnixTimestamp() + 60;
                GetClient().GetHabbo().MessengerSpamCount = 0;
                GetClient()
                    .SendNotification(
                        "You cannot send a message, you have flooded the console.\n\nYou can send a message in 60 seconds.");
                return;
            }
            if (GetClient().GetHabbo().MessengerSpamTime > OblivionServer.GetUnixTimestamp())
            {
                var Time = GetClient().GetHabbo().MessengerSpamTime - OblivionServer.GetUnixTimestamp();
                GetClient()
                    .SendNotification(
                        "You cannot send a message, you have flooded the console.\n\nYou can send a message in " + Time +
                        " seconds.");
                return;
            }


            GetClient().GetHabbo().MessengerSpamCount++;

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(ToId);
            if (Client?.GetHabbo() == null || Client.GetHabbo().GetMessenger() == null)
            {
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "INSERT INTO `messenger_offline_messages` (`to_id`, `from_id`, `message`, `timestamp`) VALUES (@tid, @fid, @msg, UNIX_TIMESTAMP())");
                    dbClient.AddParameter("tid", ToId);
                    dbClient.AddParameter("fid", GetClient().GetHabbo().Id);
                    dbClient.AddParameter("msg", Message);
                    dbClient.RunQuery();
                }
                return;
            }

            if (!Client.GetHabbo().AllowConsoleMessages ||
                Client.GetHabbo().MutedUsers.Contains(GetClient().GetHabbo().Id))
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.FRIEND_BUSY, ToId));
                return;
            }

            if (GetClient().GetHabbo().TimeMuted > 0)
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.YOUR_MUTED, ToId));
                return;
            }

            if (Client.GetHabbo().TimeMuted > 0)
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.FRIEND_MUTED, ToId));

            if (string.IsNullOrEmpty(Message))
                return;

            Client.SendMessage(new NewConsoleMessageComposer(_userId, Message));
        }


        public ServerPacket SerializeUpdate(MessengerBuddy friend)
        {
            var Packet = new ServerPacket(ServerPacketHeader.FriendListUpdateMessageComposer);
            Packet.WriteInteger(0); // category count
            Packet.WriteInteger(1); // number of updates
            Packet.WriteInteger(0); // don't know

            friend.Serialize(Packet, GetClient());
            return Packet;
        }

        public void BroadcastAchievement(int UserId, MessengerEventTypes Type, string Data)
        {
            var MyFriends = OblivionServer.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (
                var Client in
                MyFriends.ToList()
                    .Where(Client => Client.GetHabbo() != null && Client.GetHabbo().GetMessenger() != null))
            {
                Client.SendMessage(new FriendNotificationComposer(UserId, Type, Data));
                Client.GetHabbo().GetMessenger().OnStatusChanged(true);
            }
        }

        public void ClearRequests() => _requests.Clear();

        private GameClient GetClient() => OblivionServer.GetGame().GetClientManager().GetClientByUserID(_userId);

        public ICollection<MessengerRequest> GetRequests() => _requests.Values;

        public ICollection<MessengerBuddy> GetFriends() => _friends.Values;
    }
}