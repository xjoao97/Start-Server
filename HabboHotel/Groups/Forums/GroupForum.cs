#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Groups.Forums
{
    public class GroupForum
    {
        public Group Group;
        public int GroupId;
        public GroupForumSettings Settings;
        public List<GroupForumThread> Threads;


        public GroupForum(Group group)
        {
            GroupId = group.Id;
            Group = group;
            Settings = new GroupForumSettings(this);
            Threads = new List<GroupForumThread>();

            LoadThreads();
        }


        public int Id => GroupId;

        public string Name => Group.Name;

        public string Description => Group.Description;

        public int MessagesCount => Threads.SelectMany(c => c.Posts).Count();

        private void LoadThreads()
        {
            DataTable table;
            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.SetQuery("SELECT * FROM group_forums_threads WHERE forum_id = @id ORDER BY id DESC");
                adap.AddParameter("id", Id);
                table = adap.GetTable();
            }

            foreach (DataRow Row in table.Rows)
                Threads.Add(new GroupForumThread(this, Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["user_id"]),
                    Convert.ToInt32(Row["timestamp"]), Row["caption"].ToString(), Convert.ToInt32(Row["pinned"]) == 1,
                    Convert.ToInt32(Row["locked"]) == 1, Convert.ToInt32(Row["deleted_level"]),
                    Convert.ToInt32(Row["deleter_user_id"])));
        }

        public int UnreadMessages(int userid)
        {
            var i = 0;
            Threads.ForEach(c => i += c.GetUnreadMessages(userid));
            return i;
        }

        public GroupForumThreadPost GetLastPost()
        {
            var Posts = Threads.SelectMany(c => c.Posts);
            return Posts.OrderByDescending(c => c.Timestamp).FirstOrDefault();
        }

        public GroupForumThread GetThread(int ThreadId) => Threads.FirstOrDefault(c => c.Id == ThreadId);

        public GroupForumThread CreateThread(int Creator, string Caption)
        {
            var timestamp = (int) OblivionServer.GetUnixTimestamp();
            var Thread = new GroupForumThread(this, 0, Creator, timestamp, Caption, false, false, 0, 0);

            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.SetQuery(
                    "INSERT INTO group_forums_threads (forum_id, user_id, caption, timestamp) VALUES (@a, @b, @c, @d)");
                adap.AddParameter("a", Id);
                adap.AddParameter("b", Creator);
                adap.AddParameter("c", Caption);
                adap.AddParameter("d", timestamp);
                Thread.Id = (int) adap.InsertQuery();
            }

            Threads.Add(Thread);
            return Thread;
        }

        public GroupForumThreadPost GetPost(int postid)
            => Threads.SelectMany(c => c.Posts).FirstOrDefault(c => c.Id == postid);
    }
}