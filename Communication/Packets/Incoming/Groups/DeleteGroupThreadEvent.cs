#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class DeleteGroupThreadEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var int1 = Packet.PopInt();
            var int2 = Packet.PopInt();
            var int3 = Packet.PopInt();

            var forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(int1);

            if (forum == null)
            {
                Session.SendNotification("Not Found");
                return;
            }

            if (forum.Settings.GetReasonForNot(Session, forum.Settings.WhoCanModerate) != "")
            {
                Session.SendNotification("You don't have perms");
                return;
            }

            var thread = forum.GetThread(int2);
            if (thread == null)
            {
                Session.SendNotification("Not found");
                return;
            }

            thread.DeletedLevel = int3 / 10;

            thread.DeleterUserId = thread.DeletedLevel != 0 ? Session.GetHabbo().Id : 0;

            thread.Save();

            Session.SendMessage(new ThreadsListDataComposer(forum, Session));

            Session.SendMessage(thread.DeletedLevel != 0
                ? new RoomNotificationComposer("forums.thread.hidden")
                : new RoomNotificationComposer("forums.thread.restored"));
        }
    }
}