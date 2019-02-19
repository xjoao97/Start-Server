#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class ManageGroupEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            if (Group.CreatorId != Session.GetHabbo().Id &&
                !Session.GetHabbo().GetPermissions().HasRight("group_management_override"))
                return;

            Session.SendMessage(new ManageGroupComposer(Group));
        }
    }
}