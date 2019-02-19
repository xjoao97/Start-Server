#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GetThreadDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ForumId = Packet.PopInt(); //Maybe Forum ID
            var ThreadId = Packet.PopInt(); //Maybe Thread ID
            var StartIndex = Packet.PopInt(); //Start index
            var length = Packet.PopInt(); //List Length

            var Forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(ForumId);

            if (Forum == null)
            {
                Session.SendNotification(";forum.thread.open.error.forumnotfound");
                return;
            }

            var Thread = Forum.GetThread(ThreadId);
            if (Thread == null)
            {
                Session.SendNotification(";forum.thread.open.error.threadnotfound");
                return;
            }

            if (Thread.DeletedLevel > 1 &&
                Forum.Settings.GetReasonForNot(Session, Forum.Settings.WhoCanModerate) != "")
            {
                Session.SendNotification(";forum.thread.open.error.deleted");
                return;
            }


            Session.SendMessage(new ThreadDataComposer(Thread, StartIndex, length));
        }
    }
}