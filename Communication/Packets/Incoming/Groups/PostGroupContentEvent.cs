#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class PostGroupContentEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ForumId = Packet.PopInt();
            var ThreadId = Packet.PopInt();
            var Caption = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Message = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());

            var Forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(ForumId);
            if (Forum == null)
            {
                Session.SendNotification(";forum.thread.post.reply.error.forumnotfound");
                return;
            }
            var e = "";
            var IsNewThread = ThreadId == 0;
            if (IsNewThread)
            {
                if ((e = Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanInitDiscussions)) != "")
                {
                    Session.SendNotification(";forum.thread.create.error." + e);
                    return;
                }

                var Thread = Forum.CreateThread(Session.GetHabbo().Id, Caption);
                var Post = Thread.CreatePost(Session.GetHabbo().Id, Message);

                Session.SendMessage(new ThreadCreatedComposer(Session, Thread));

                Session.SendMessage(new ThreadDataComposer(Thread, 0, 2000));

                //Session.SendMessage(new PostUpdatedComposer(Session, Post));
                //Session.SendMessage(new ThreadReplyComposer(Session, Post));

                Thread.AddView(Session.GetHabbo().Id, 1);
            }
            else
            {
                var Thread = Forum.GetThread(ThreadId);
                if (Thread == null)
                {
                    Session.SendNotification(";forum.thread.post.reply.error.threadnotfound");
                    return;
                }

                if (Thread.Locked && Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanModerate) != "")
                {
                    Session.SendNotification(";forum.thread.post.reply.error.locked");
                    return;
                }

                if ((e = Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanPost)) != "")
                {
                    Session.SendNotification(";forum.thread.post.reply.error." + e);
                    return;
                }

                var Post = Thread.CreatePost(Session.GetHabbo().Id, Message);
                Session.SendMessage(new ThreadReplyComposer(Session, Post));
            }
        }
    }
}