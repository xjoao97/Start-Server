#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Rooms.Permissions;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);

            if (Group?.CreatorId != Session.GetHabbo().Id)
                return;

            var Type = Packet.PopInt();
            var FurniOptions = Packet.PopInt();

            switch (Type)
            {
                default:
                case 0:
                    Group.GroupType = GroupType.OPEN;
                    break;
                case 1:
                    Group.GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    Group.GroupType = GroupType.PRIVATE;
                    break;
            }

            if (Group.GroupType != GroupType.LOCKED)
                if (Group.GetRequests.Count > 0)
                {
                    foreach (var UserId in Group.GetRequests.ToList())
                        Group.HandleRequest(UserId, false);

                    Group.ClearRequests();
                }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `groups` SET `state` = @GroupState, `admindeco` = @AdminDeco WHERE `id` = " +
                                  Group.Id + " LIMIT 1");
                dbClient.AddParameter("GroupState",
                    (Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2).ToString());
                dbClient.AddParameter("AdminDeco", (FurniOptions == 1 ? 1 : 0).ToString());
                dbClient.RunQuery();
            }

            Group.AdminOnlyDeco = FurniOptions;

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
                return;

            foreach (
                var User in
                Room.GetRoomUserManager()
                    .GetRoomUsers()
                    .ToList()
                    .Where(
                        User =>
                            Room.OwnerId != User.UserId && !Group.IsAdmin(User.UserId) && Group.IsMember(User.UserId)))
                if (FurniOptions == 1)
                {
                    User.RemoveStatus("flatctrl 1");
                    User.UpdateNeeded = true;

                    User.GetClient().SendMessage(new YouAreControllerComposer(0));
                }
                else if (FurniOptions == 0 && !User.Statusses.ContainsKey("flatctrl 1"))
                {
                    User.AddStatus("flatctrl 1", "");
                    User.UpdateNeeded = true;

                    User.GetClient().SendMessage(new YouAreControllerComposer(1));
                }

            Session.SendMessage(new GroupInfoComposer(Group, Session));
        }
    }
}