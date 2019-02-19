#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GetThreadsListDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ForumId = Packet.PopInt(); //Forum ID
            var Int2 = Packet.PopInt(); //Start Index of Thread Count
            var Int3 = Packet.PopInt(); //Length of Thread Count

            var Forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(ForumId);
            if (Forum == null)
                return;

            Session.SendMessage(new ThreadsListDataComposer(Forum, Session, Int2, Int3));
        }
    }
}