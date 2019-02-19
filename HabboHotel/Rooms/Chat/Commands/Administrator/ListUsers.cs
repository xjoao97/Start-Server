/*#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class ListUsers : IChatCommand
    {
        public string PermissionRequired => "command_hal";

        public string Parameters => "";

        public string Description => "List users";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            foreach (var user in room.GetRoomUserManager().GetRoomUsers())
                session.SendNotification(user.GetUsername());
        }
    }
}*/