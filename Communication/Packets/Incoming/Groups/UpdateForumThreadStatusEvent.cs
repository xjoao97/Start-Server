#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    public class UpdateForumThreadStatusEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ForumID = Packet.PopInt();
            var ThreadID = Packet.PopInt();
            var Pinned = Packet.PopBoolean();
            var Locked = Packet.PopBoolean();


            var forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(ForumID);
            var thread = forum.GetThread(ThreadID);

            if (forum.Settings.GetReasonForNot(Session, forum.Settings.WhoCanModerate) != "")
            {
                Session.SendNotification("You don't haver perms to this.");
                return;
            }

            bool isPining = thread.Pinned != Pinned,
                isLocking = thread.Locked != Locked;

            thread.Pinned = Pinned;
            thread.Locked = Locked;

            thread.Save();

            Session.SendMessage(new ThreadUpdatedComposer(Session, thread));

            if (isPining)
                Session.SendMessage(Pinned
                    ? new RoomNotificationComposer("forums.thread.pinned")
                    : new RoomNotificationComposer("forums.thread.unpinned"));

            if (isLocking)
                Session.SendMessage(Locked
                    ? new RoomNotificationComposer("forums.thread.locked")
                    : new RoomNotificationComposer("forums.thread.unlocked"));
        }
    }
}