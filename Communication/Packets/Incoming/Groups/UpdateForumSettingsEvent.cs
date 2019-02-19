#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class UpdateForumSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ForumId = Packet.PopInt();
            var WhoCanRead = Packet.PopInt();
            var WhoCanReply = Packet.PopInt();
            var WhoCanPost = Packet.PopInt();
            var WhoCanMod = Packet.PopInt();


            var forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(ForumId);

            if (forum == null)
            {
                Session.SendNotification("Forums not found");
                return;
            }

            if (forum.Settings.GetReasonForNot(Session, forum.Settings.WhoCanModerate) != "")
            {
                Session.SendNotification("You don't have perms.");
                return;
            }

            forum.Settings.WhoCanRead = WhoCanRead;
            forum.Settings.WhoCanModerate = WhoCanMod;
            forum.Settings.WhoCanPost = WhoCanReply;
            forum.Settings.WhoCanInitDiscussions = WhoCanPost;

            forum.Settings.Save();

            Session.SendMessage(new ForumDataComposer(forum, Session));
            Session.SendMessage(new ThreadsListDataComposer(forum, Session));
        }
    }
}