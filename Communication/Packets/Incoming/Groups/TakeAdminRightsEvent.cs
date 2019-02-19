#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Rooms.Permissions;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class TakeAdminRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var UserId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);

            if (Session.GetHabbo().Id != Group?.CreatorId || !Group.IsMember(UserId))
                return;

            var Habbo = OblivionServer.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            Group.TakeAdmin(UserId);

            Room Room = null;
            if (OblivionServer.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
            {
                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (User != null)
                {
                    if (User.Statusses.ContainsKey("flatctrl 3"))
                        User.Statusses["flatctrl 3"] = null;
                    User.UpdateNeeded = true;
                    if (User.GetClient() != null)
                        User.GetClient().SendMessage(new YouAreControllerComposer(0));
                }
            }

            Session.SendMessage(new GroupMemberUpdatedComposer(GroupId, Habbo, 2));
        }
    }
}