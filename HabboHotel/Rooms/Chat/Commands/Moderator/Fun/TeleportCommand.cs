#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class TeleportCommand : IChatCommand
    {
        public string PermissionRequired => "command_teleport";

        public string Parameters => "";

        public string Description => "The ability to teleport anywhere within the room.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.TeleportEnabled = !user.TeleportEnabled;
            room.GetGameMap().GenerateMaps();
        }
    }
}