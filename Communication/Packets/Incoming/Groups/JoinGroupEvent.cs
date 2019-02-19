#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class JoinGroupEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(Packet.PopInt());
            if (Group == null)
                return;

            if (Group.IsMember(Session.GetHabbo().Id) || Group.IsAdmin(Session.GetHabbo().Id) ||
                Group.HasRequest(Session.GetHabbo().Id) && Group.GroupType == GroupType.PRIVATE)
                return;

            var Groups = OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id);
            if (Groups.Count >= 1500)
            {
                Session.SendMessage(
                    new BroadcastMessageAlertComposer(
                        "Oops, it appears that you've hit the group membership limit! You can only join upto 1,500 groups."));
                return;
            }

            Group.AddMember(Session.GetHabbo().Id);

            if (Group.GroupType == GroupType.LOCKED)
            {
                var GroupAdmins = (from Client in OblivionServer.GetGame().GetClientManager().GetClients.ToList()
                    where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id)
                    select Client).ToList();
                foreach (var Client in GroupAdmins)
                    Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, Session.GetHabbo(), 3));

                Session.SendMessage(new GroupInfoComposer(Group, Session));
            }
            else
            {
                Session.SendMessage(
                    new GroupFurniConfigComposer(
                        OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id)));
                Session.SendMessage(new GroupInfoComposer(Group, Session));

                if (Session.GetHabbo().CurrentRoom != null)
                    Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                else
                    Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                if (Group.HasChat)
                    Session.SendMessage(new FriendListUpdateComposer(Group));
            }
        }
    }
}