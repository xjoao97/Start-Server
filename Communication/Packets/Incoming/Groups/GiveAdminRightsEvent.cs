#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Rooms.Permissions;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class GiveAdminRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var UserId = Packet.PopInt();

            var Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);

            if (Session.GetHabbo().Id != Group?.CreatorId || !Group.IsMember(UserId))
                return;

            var Habbo = OblivionServer.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            Group.MakeAdmin(UserId);

            Room Room;
            if (OblivionServer.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
            {
                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (User != null)
                {
                    if (!User.Statusses.ContainsKey("flatctrl 3"))
                        User.SetStatus("flatctrl 3", "");
                    User.UpdateNeeded = true;
                    if (User.GetClient() != null)
                        User.GetClient().SendMessage(new YouAreControllerComposer(3));
                }
            }

            Session.SendMessage(new GroupMemberUpdatedComposer(GroupId, Habbo, 1));
        }
    }
}