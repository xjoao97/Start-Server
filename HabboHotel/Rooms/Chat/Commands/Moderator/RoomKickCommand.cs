#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomKickCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_kick";

        public string Parameters => "%message%";

        public string Description => "Expulse todos os usuários da sala.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Expulse todos os usuários da sala.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var message = CommandManager.MergeParams(Params, 1);
            foreach (
                var roomUser in
                room.GetRoomUserManager()
                    .GetUserList()
                    .ToList()
                    .Where(
                        roomUser =>
                            roomUser?.GetClient() != null && !roomUser.IsBot && roomUser.GetClient().GetHabbo() != null &&
                            !roomUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") &&
                            roomUser.GetClient().GetHabbo().Id != session.GetHabbo().Id))
            {
                roomUser.GetClient().SendNotification("Você foi expulso da sala pelo seguinte motivo: " + message);

                room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true);
            }

            session.SendWhisper("Sucesso, você expulsou toda a sala.");
        }
    }
}