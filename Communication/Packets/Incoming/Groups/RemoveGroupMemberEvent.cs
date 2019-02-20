#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.Communication.Packets.Outgoing.Rooms.Permissions;
using Oblivion.HabboHotel.Cache;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class RemoveGroupMemberEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GroupId = Packet.PopInt();
            var UserId = Packet.PopInt();

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            if (UserId == Session.GetHabbo().Id)
            {
                if (Group.IsMember(UserId))
                    Group.DeleteMember(UserId);

                if (Group.IsAdmin(UserId))
                {
                    if (Group.IsAdmin(UserId))
                        Group.TakeAdmin(UserId);

                    Room Room;

                    if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
                        return;

                    var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (User != null)
                    {
                        User.RemoveStatus("flatctrl 1");
                        User.UpdateNeeded = true;

                        if (User.GetClient() != null)
                            User.GetClient().SendMessage(new YouAreControllerComposer(0));
                    }
                }

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "DELETE FROM `group_memberships` WHERE `group_id` = @GroupId AND `user_id` = @UserId");
                    dbClient.AddParameter("GroupId", GroupId);
                    dbClient.AddParameter("UserId", UserId);
                    dbClient.RunQuery();
                }

                Session.SendMessage(new GroupInfoComposer(Group, Session));
                if (Session.GetHabbo().GetStats().FavouriteGroupId == GroupId)
                {
                    Session.GetHabbo().GetStats().FavouriteGroupId = 0;
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.runFastQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `id` = '" + UserId +
                                          "' LIMIT 1");
                    }

                    if (Group.AdminOnlyDeco == 0)
                    {
                        Room Room;
                        if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
                            return;

                        var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (User != null)
                        {
                            User.RemoveStatus("flatctrl 1");
                            User.UpdateNeeded = true;

                            if (User.GetClient() != null)
                                User.GetClient().SendMessage(new YouAreControllerComposer(0));
                        }
                    }

                    if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoom != null)
                    {
                        var User =
                            Session.GetHabbo()
                                .CurrentRoom.GetRoomUserManager()
                                .GetRoomUserByHabbo(Session.GetHabbo().Id);
                        if (User != null)
                            Session.GetHabbo()
                                .CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Group,
                                    User.VirtualId));
                        Session.GetHabbo()
                            .CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                    }
                    else
                    {
                        Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                    }
                }
            }
            else
            {
                if (Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id))
                {
                    if (!Group.IsMember(UserId))
                        return;

                    if (Group.IsAdmin(UserId) && Group.CreatorId != Session.GetHabbo().Id)
                    {
                        Session.SendNotification(
                            "Sorry, only group creators can remove other administrators from the group.");
                        return;
                    }

                    if (Group.IsAdmin(UserId))
                        Group.TakeAdmin(UserId);

                    if (Group.IsMember(UserId))
                        Group.DeleteMember(UserId);

                    var Members = new List<UserCache>();
                    var MemberIds = Group.GetAllMembers;
                    foreach (
                        var GroupMember in
                        MemberIds.ToList()
                            .Select(Id => OblivionServer.GetGame().GetCacheManager().GenerateUser(Id))
                            .Where(GroupMember => GroupMember != null)
                            .Where(GroupMember => !Members.Contains(GroupMember)))
                        Members.Add(GroupMember);


                    var FinishIndex = 14 < Members.Count ? 14 : Members.Count;
                    var MembersCount = Members.Count;

                    Session.SendMessage(new GroupMembersComposer(Group, Members.Take(FinishIndex).ToList(), MembersCount,
                        1, Group.CreatorId == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id), 0, ""));
                    if (Group.HasChat)
                        Session.SendMessage(new FriendListUpdateComposer(-Group.Id));
                }
            }
        }
    }
}