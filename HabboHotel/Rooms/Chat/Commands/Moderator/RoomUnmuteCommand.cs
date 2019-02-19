#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomUnmuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_unroommute";

        public string Parameters => "";

        public string Description => "Hora de ouvir.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.RoomMuted)
            {
                session.SendWhisper("O quarto não está mais silenciado.");
                return;
            }

            room.RoomMuted = false;

            var roomUsers = room.GetRoomUserManager().GetRoomUsers();
            if (roomUsers.Count > 0)
                foreach (
                    var user in
                    roomUsers.Where(
                        user =>
                            user?.GetClient() != null && user.GetClient().GetHabbo() != null &&
                            user.GetClient().GetHabbo().Username != session.GetHabbo().Username))
                    user.GetClient().SendWhisper("O quarto não está mais silenciado.");
        }
    }
}