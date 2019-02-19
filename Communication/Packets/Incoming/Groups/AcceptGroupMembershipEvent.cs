#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class AcceptGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var UserId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            if (Session.GetHabbo().Id != Group.CreatorId && !Group.IsAdmin(Session.GetHabbo().Id) &&
                !Session.GetHabbo().GetPermissions().HasRight("fuse_group_accept_any"))
                return;

            if (!Group.HasRequest(UserId))
                return;

            var Habbo = OblivionServer.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            Group.HandleRequest(UserId, true);

            Session.SendMessage(new GroupMemberUpdatedComposer(GroupId, Habbo, 4));
        }
    }
}