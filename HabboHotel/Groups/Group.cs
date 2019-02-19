#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Groups
{
    public class Group
    {
        private readonly List<int> _administrators;

        public List<int> ChatUsers;
        private readonly List<int> _members;
        private readonly List<int> _requests;

        public Group(int Id, string Name, string Description, string Badge, int RoomId, int Owner, int Time, int Type,
            int Colour1, int Colour2, int AdminOnlyDeco, bool HasForum, bool HasChat)
        {
            this.Id = Id;
            this.Name = Name;
            this.Description = Description;
            this.RoomId = RoomId;
            this.Badge = Badge;
            CreateTime = Time;
            CreatorId = Owner;
            this.Colour1 = Colour1 == 0 ? 1 : Colour1;
            this.Colour2 = Colour2 == 0 ? 1 : Colour2;
            this.HasForum = HasForum;
            this.HasChat = HasChat;
            switch (Type)
            {
                case 0:
                    GroupType = GroupType.OPEN;
                    break;
                case 1:
                    GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    GroupType = GroupType.PRIVATE;
                    break;
            }

            this.AdminOnlyDeco = AdminOnlyDeco;
            ForumEnabled = ForumEnabled;

            _members = new List<int>();
            _requests = new List<int>();
            _administrators = new List<int>();
            ChatUsers = new List<int>();
            InitMembers();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int AdminOnlyDeco { get; set; }
        public string Badge { get; set; }
        public int CreateTime { get; set; }
        public int CreatorId { get; set; }
        public string Description { get; set; }
        public int RoomId { get; set; }
        public int Colour1 { get; set; }
        public int Colour2 { get; set; }
        public bool ForumEnabled { get; set; }
        public GroupType GroupType { get; set; }
        public bool HasForum { get; set; }
        public bool HasChat { get; set; }

        public List<int> GetMembers => _members.ToList();

        public List<int> GetRequests => _requests.ToList();

        public List<int> GetAdministrators => _administrators.ToList();

        public List<int> GetAllMembers
        {
            get
            {
                var Members = new List<int>(_administrators.ToList());
                Members.AddRange(_members.ToList());

                return Members;
            }
        }


        public int MemberCount => _members.Count + _administrators.Count;

        public int RequestCount => _requests.Count;

        public void InitMembers()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetMembers;
                dbClient.SetQuery("SELECT `user_id`, `rank`, `has_chat` FROM `group_memberships` WHERE `group_id` = @id");
                dbClient.AddParameter("id", Id);
                GetMembers = dbClient.getTable();

                if (GetMembers != null)
                    foreach (DataRow Row in GetMembers.Rows)
                    {
                        var UserId = Convert.ToInt32(Row["user_id"]);
                        var IsAdmin = Convert.ToInt32(Row["rank"]) != 0;
                        var hasChat = Convert.ToInt32(Row["has_chat"]) == 1;
                        if (hasChat && !ChatUsers.Contains(UserId))
                            ChatUsers.Add(UserId);

                        if (IsAdmin)
                        {
                            if (!_administrators.Contains(UserId))
                                _administrators.Add(UserId);
                        }
                        else
                        {
                            if (!_members.Contains(UserId))
                                _members.Add(UserId);
                        }
                    }

                DataTable GetRequests;
                dbClient.SetQuery("SELECT `user_id` FROM `group_requests` WHERE `group_id` = @id");
                dbClient.AddParameter("id", Id);
                GetRequests = dbClient.getTable();

                if (GetRequests == null) return;
                foreach (var UserId in from DataRow Row in GetRequests.Rows select Convert.ToInt32(Row["user_id"]))
                    if (_members.Contains(UserId) || _administrators.Contains(UserId))
                        dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Id +
                                          "' AND `user_id` = '" + UserId + "'");
                    else if (!_requests.Contains(UserId))
                        _requests.Add(UserId);
            }
        }

        public bool IsMember(int Id) => _members.Contains(Id) || _administrators.Contains(Id);

        public bool IsAdmin(int Id) => _administrators.Contains(Id);

        public bool HasRequest(int Id) => _requests.Contains(Id);

        public void MakeAdmin(int Id)
        {
            if (_members.Contains(Id))
                _members.Remove(Id);

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE group_memberships SET `rank` = '1' WHERE `user_id` = @uid AND `group_id` = @gid LIMIT 1");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }

            if (!_administrators.Contains(Id))
                _administrators.Add(Id);
        }

        public void TakeAdmin(int UserId)
        {
            if (!_administrators.Contains(UserId))
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE group_memberships SET `rank` = '0' WHERE user_id = @uid AND group_id = @gid");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", UserId);
                dbClient.RunQuery();
            }

            _administrators.Remove(UserId);
            _members.Add(UserId);
        }

        public void AddMember(int Id)
        {
            if (IsMember(Id) || GroupType == GroupType.LOCKED && _requests.Contains(Id))
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                if (IsAdmin(Id))
                {
                    dbClient.SetQuery(
                        "UPDATE `group_memberships` SET `rank` = '0' WHERE user_id = @uid AND group_id = @gid");
                    _administrators.Remove(Id);
                    _members.Add(Id);
                }
                else if (GroupType == GroupType.LOCKED)
                {
                    dbClient.SetQuery("INSERT INTO `group_requests` (user_id, group_id) VALUES (@uid, @gid)");
                    _requests.Add(Id);
                }
                else
                {
                    dbClient.SetQuery("INSERT INTO `group_memberships` (user_id, group_id) VALUES (@uid, @gid)");
                    _members.Add(Id);
                }

                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }
        }

        public void DeleteMember(int Id)
        {
            if (IsMember(Id))
            {
                if (_members.Contains(Id))
                    _members.Remove(Id);
            }
            else if (IsAdmin(Id))
            {
                if (_administrators.Contains(Id))
                    _administrators.Remove(Id);
            }
            else
            {
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM group_memberships WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }
        }

        public void HandleRequest(int Id, bool Accepted)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                if (Accepted)
                {
                    dbClient.SetQuery("INSERT INTO group_memberships (user_id, group_id) VALUES (@uid, @gid)");
                    dbClient.AddParameter("gid", this.Id);
                    dbClient.AddParameter("uid", Id);
                    dbClient.RunQuery();

                    _members.Add(Id);
                }

                dbClient.SetQuery("DELETE FROM group_requests WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", this.Id);
                dbClient.AddParameter("uid", Id);
                dbClient.RunQuery();
            }

            if (_requests.Contains(Id))
                _requests.Remove(Id);
        }

        public void ClearRequests() => _requests.Clear();

        public void Dispose()
        {
            _requests.Clear();
            _members.Clear();
            _administrators.Clear();
        }
    }
}