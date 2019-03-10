#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class DeleteGroupEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(Packet.PopInt());
            if (Group == null)
            {
                Session.SendNotification("Oops, we couldn't find that group!");
                return;
            }

            if (Group.CreatorId != Session.GetHabbo().Id &&
                !Session.GetHabbo().GetPermissions().HasRight("group_delete_override"))
                //Maybe a FUSE check for staff override?
            {
                Session.SendNotification("Oops, only the group owner can delete a group!");
                return;
            }

            if (Group.MemberCount >= OblivionStaticGameSettings.GroupMemberDeletionLimit &&
                !Session.GetHabbo().GetPermissions().HasRight("group_delete_limit_override"))
            {
                Session.SendNotification("Oops, your group exceeds the maximum amount of members (" +
                                         OblivionStaticGameSettings.GroupMemberDeletionLimit +
                                         ") a group can exceed before being eligible for deletion. Seek assistance from a staff member.");
                return;
            }

            var Room = OblivionServer.GetGame().GetRoomManager().LoadRoom(Group.RoomId);

            if (Room != null)
            {
                Room.Group = null;
                Room.RoomData.Group = null; //I'm not sure if this is needed or not, becauseof inheritance, but oh well.
            }

            //Remove it from the cache.
            OblivionServer.GetGame().GetGroupManager().DeleteGroup(Group.Id);

            //Now the :S stuff.
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM `groups` WHERE `id` = '" + Group.Id + "'");
                dbClient.RunFastQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + Group.Id + "'");
                dbClient.RunFastQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Group.Id + "'");
                dbClient.RunFastQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + Group.Id + "' LIMIT 1");
                dbClient.RunFastQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" + Group.Id + "' LIMIT 1");
                dbClient.RunFastQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + Group.Id + "'");
            }

            //Unload it last.
            OblivionServer.GetGame().GetRoomManager().UnloadRoom(Room, true);

            //Say hey!
            Session.SendNotification("You have successfully deleted your group.");
        }
    }
}