#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomMuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_roommute";

        public string Parameters => "%message%";

        public string Description => "Silencie o quarto com uma razão.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, digite uma mensagem de razão para os usuários.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            if (!room.RoomMuted)
                room.RoomMuted = true;

            var msg = CommandManager.MergeParams(Params, 1);

            var roomUsers = room.GetRoomUserManager().GetRoomUsers();
            if (roomUsers.Count > 0)
                foreach (
                    var user in
                    roomUsers.Where(
                        user =>
                            user?.GetClient() != null && user.GetClient().GetHabbo() != null &&
                            user.GetClient().GetHabbo().Username != session.GetHabbo().Username))
                    user.GetClient().SendWhisper("O quarto foi silenciado pela seguinte razão: " + msg);
        }
    }
}