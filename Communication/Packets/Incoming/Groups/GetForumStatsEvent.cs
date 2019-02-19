#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups.Forums;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GetForumStatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupForumId = Packet.PopInt();

            GroupForum Forum;
            if (!OblivionServer.GetGame().GetGroupForumManager().TryGetForum(GroupForumId, out Forum))
            {
                Session.SendNotification("Opss, Forum inexistente!");
                return;
            }

            Session.SendMessage(new ForumDataComposer(Forum, Session));
        }
    }
}