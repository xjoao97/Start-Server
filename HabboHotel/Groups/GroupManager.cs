#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using log4net;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Groups
{
    public class GroupManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Groups.GroupManager");
        private HybridDictionary _groups;

        public HybridDictionary BackGroundColours;
        public List<GroupBaseColours> BaseColours;
        public List<GroupBases> Bases;

        public HybridDictionary SymbolColours;
        public List<GroupSymbols> Symbols;

        public GroupManager()
        {
            Init();
        }

        public Group TryGetGroup(int Id)
        {

            if (_groups.Contains(Id))
                return (Group)_groups[Id];
            
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `groups` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                var Row = dbClient.getRow();

                if (Row == null) return null;
               var Group = new Group(
                    Convert.ToInt32(Row["id"]), Convert.ToString(Row["name"]), Convert.ToString(Row["desc"]),
                    Convert.ToString(Row["badge"]), Convert.ToInt32(Row["room_id"]),
                    Convert.ToInt32(Row["owner_id"]),
                    Convert.ToInt32(Row["created"]), Convert.ToInt32(Row["state"]),
                    Convert.ToInt32(Row["colour1"]), Convert.ToInt32(Row["colour2"]),
                    Convert.ToInt32(Row["admindeco"]), Convert.ToInt32(Row["has_forum"]) == 1,
                    Convert.ToInt32(Row["has_chat"]) == 1);
                _groups.Add(Group.Id, Group);
                return Group;
            }
        }

        public void Init()
        {
            Bases = new List<GroupBases>();
            Symbols = new List<GroupSymbols>();
            BaseColours = new List<GroupBaseColours>();
            SymbolColours = new HybridDictionary();
            BackGroundColours = new HybridDictionary();
            _groups = new HybridDictionary();

            ClearInfo();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM groups_items WHERE enabled='1'");
                var dItems = dbClient.getTable();

                foreach (DataRow dRow in dItems.Rows)
                    switch (dRow[0].ToString())
                    {
                        case "base":
                            Bases.Add(new GroupBases(Convert.ToInt32(dRow[1]), dRow[2].ToString(), dRow[3].ToString()));
                            break;

                        case "symbol":
                            Symbols.Add(new GroupSymbols(Convert.ToInt32(dRow[1]), dRow[2].ToString(),
                                dRow[3].ToString()));
                            break;

                        case "color":
                            BaseColours.Add(new GroupBaseColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;

                        case "color2":
                            SymbolColours.Add(Convert.ToInt32(dRow[1]),
                                new GroupSymbolColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;

                        case "color3":
                            BackGroundColours.Add(Convert.ToInt32(dRow[1]),
                                new GroupBackGroundColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;
                    }
            }

            log.Info("Group Manager -> LOADED");
        }

        public void ClearInfo()
        {
            Bases.Clear();
            Symbols.Clear();
            BaseColours.Clear();
            SymbolColours.Clear();
            BackGroundColours.Clear();
        }

        public void TryCreateGroup(Habbo Player, string Name, string Description, int RoomId, string Badge, int Colour1,
            int Colour2, out Group Group)
        {
            Group = new Group(0, Name, Description, Badge, RoomId, Player.Id, (int) OblivionServer.GetUnixTimestamp(),
                0, Colour1, Colour2, 0, false, false);
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Badge))
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `groups` (`name`, `desc`, `badge`, `owner_id`, `created`, `room_id`, `state`, `colour1`, `colour2`, `admindeco`) VALUES (@name, @desc, @badge, @owner, UNIX_TIMESTAMP(), @room, '0', @colour1, @colour2, '0')");
                dbClient.AddParameter("name", Group.Name);
                dbClient.AddParameter("desc", Group.Description);
                dbClient.AddParameter("owner", Group.CreatorId);
                dbClient.AddParameter("badge", Group.Badge);
                dbClient.AddParameter("room", Group.RoomId);
                dbClient.AddParameter("colour1", Group.Colour1);
                dbClient.AddParameter("colour2", Group.Colour2);
                Group.Id = Convert.ToInt32(dbClient.InsertQuery());

                Group.AddMember(Player.Id);
                Group.MakeAdmin(Player.Id);

                _groups.Add(Group.Id, Group);

                dbClient.SetQuery("UPDATE `rooms` SET `group_id` = @gid WHERE `id` = @rid LIMIT 1");
                dbClient.AddParameter("gid", Group.Id);
                dbClient.AddParameter("rid", Group.RoomId);
                dbClient.RunQuery();

                dbClient.RunFastQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + RoomId + "'");
            }
        }


        public string CheckActiveSymbol(string Symbol)
        {
            if (Symbol == "s000" || Symbol == "s00000")
                return "";
            return Symbol;
        }

        public string GetGroupColour(int Index, bool Colour1)
        {
            if (Colour1)
            {
                if (SymbolColours.Contains(Index))
                    return ((GroupSymbolColours)SymbolColours[Index]).Colour;
            }
            else
            {
                if (BackGroundColours.Contains(Index))
                    return ((GroupBackGroundColours)BackGroundColours[Index]).Colour;
            }

            return "4f8a00";
        }

        public void DeleteGroup(Group Group)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM `groups` WHERE `id` = '" + Group.Id + "'");
                dbClient.RunFastQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + Group.Id + "'");


                dbClient.RunFastQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Group.Id + "'");
                dbClient.RunFastQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + Group.Id + "' LIMIT 1");
                dbClient.RunFastQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" + Group.Id + "' LIMIT 1");
            }
            
            _groups.Remove(Group.Id);

            Group.Dispose();
        }

        public void DeleteGroup(int id)
        {
            if (_groups.Contains(id))
            {
                var group = (Group) _groups[id];
                _groups.Remove(id);

                group?.Dispose();
            }
        }


        public List<Group> GetGroupsForUser(int UserId, bool IsOwner = false)
        {
            var Groups = new List<Group>();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE m.user_id = @user");
                dbClient.AddParameter("user", UserId);
                var GetGroups = dbClient.getTable();

                if (GetGroups != null)
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        var Group = TryGetGroup(Convert.ToInt32(Row["id"]));
                        if (Group != null &&
                            (IsOwner && Group.CreatorId == UserId || !IsOwner))
                            Groups.Add(Group);
                    }
            }
            return Groups;
        }
    }
}