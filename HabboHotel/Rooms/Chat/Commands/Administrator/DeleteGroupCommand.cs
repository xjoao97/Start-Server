#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class DeleteGroupCommand : IChatCommand
    {
        public string PermissionRequired => "command_delete_group";

        public string Parameters => "";

        public string Description => "Delete a group from the database and cache.";

        public void Execute(GameClient session, string[] Params)
        {
            var Room = session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (Room.Group == null)
            {
                session.SendWhisper("Oops, there is no group here?");
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `groups` WHERE `id` = '" + Room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + Room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Room.Group.Id + "'");
                dbClient.RunQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + Room.Group.Id +
                                  "' LIMIT 1");
                dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" + Room.Group.Id +
                                  "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + Room.Group.Id + "'");
            }

            OblivionServer.GetGame().GetGroupManager().DeleteGroup(Room.RoomData.Group.Id);

            Room.Group = null;
            Room.RoomData.Group = null;

            OblivionServer.GetGame().GetRoomManager().UnloadRoom(Room, true);

            session.SendNotification("Success, group deleted.");
        }
    }
}