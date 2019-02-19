#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class DeclineGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var UserId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            if (Session.GetHabbo().Id != Group.CreatorId && !Group.IsAdmin(Session.GetHabbo().Id))
                return;

            if (!Group.HasRequest(UserId))
                return;

            Group.HandleRequest(UserId, false);
            Session.SendMessage(new UnknownGroupComposer(Group.Id, UserId));
        }
    }
}