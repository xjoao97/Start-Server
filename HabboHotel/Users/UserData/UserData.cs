#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users.Badges;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Relationships;

#endregion

namespace Oblivion.HabboHotel.Users.UserDataManagement
{
    public class UserData
    {
        public ConcurrentDictionary<string, UserAchievement> achievements;
        public List<Badge> badges;
        public List<string> BlockedCommands;
        public bool DisabledEventAlert;
        public List<int> favouritedRooms;
        public Dictionary<int, MessengerBuddy> friends;
        public List<int> ignores;
        public Dictionary<int, int> quests;

        public Dictionary<int, Relationship> Relations;
        public Dictionary<int, MessengerRequest> requests;
        public List<RoomData> rooms;
        public Habbo user;
        public int userID;

        public UserData(int userID, ConcurrentDictionary<string, UserAchievement> achievements,
            List<int> favouritedRooms, List<int> ignores,
            List<Badge> badges, Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests,
            List<RoomData> rooms, Dictionary<int, int> quests, Habbo user,
            Dictionary<int, Relationship> Relations, List<string> blockedcommands, bool disabledeventalert)
        {
            this.userID = userID;
            this.achievements = achievements;
            this.favouritedRooms = favouritedRooms;
            this.ignores = ignores;
            this.badges = badges;
            this.friends = friends;
            this.requests = requests;
            this.rooms = rooms;
            this.quests = quests;
            this.user = user;
            this.Relations = Relations;
            BlockedCommands = blockedcommands;
            DisabledEventAlert = disabledeventalert;
        }
    }
}