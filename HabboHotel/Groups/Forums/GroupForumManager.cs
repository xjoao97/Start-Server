#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Oblivion.HabboHotel.Groups.Forums
{
    public class GroupForumManager
    {
        private readonly List<GroupForum> Forums;

        public GroupForumManager()
        {
            Forums = new List<GroupForum>();
        }

        public GroupForum GetForum(int GroupId)
        {
            GroupForum f;
            return TryGetForum(GroupId, out f) ? f : null;
        }

        public bool TryGetForum(int Id, out GroupForum Forum)
        {
            if ((Forum = Forums.FirstOrDefault(c => c.Id == Id)) != null)
                return true;

            Group Gp = OblivionServer.GetGame().GetGroupManager().TryGetGroup(Id);
            if (Gp == null)
                return false;

            if (!Gp.HasForum)
                return false;
            Forum = new GroupForum(Gp);
            Forums.Add(Forum);
            return true;
        }

        public List<GroupForum> GetForumsByUserId(int Userid)
        {
            GroupForum F;
            return
                OblivionServer.GetGame().GetGroupManager()
                    .GetGroupsForUser(Userid)
                    .Where(c => TryGetForum(c.Id, out F))
                    .Select(c => GetForum(c.Id))
                    .ToList();
        }

        public void RemoveGroup(Group Group)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("DELETE FROM `group_forums_settings` WHERE `group_id` = '" + Group.Id + "'");

                dbClient.runFastQuery(
                    "DELETE post FROM group_forums_thread_posts post INNER JOIN group_forums_threads threads ON threads.forum_id = '" +
                    Group.Id + "' WHERE threads.id = post.thread_id");

                dbClient.runFastQuery(
                    "DELETE v FROM group_forums_thread_views v INNER JOIN group_forums_threads threads ON threads.forum_id = '" +
                    Group.Id + "' WHERE v.thread_id = threads.id");

                dbClient.runFastQuery("DELETE t FROM group_forums_threads t WHERE t.forum_id = '" + Group.Id + "'");
            }
        }

        public int GetUnreadThreadForumsByUserId(int Id) => GetForumsByUserId(Id).Count(c => c.UnreadMessages(Id) > 0);
    }
}