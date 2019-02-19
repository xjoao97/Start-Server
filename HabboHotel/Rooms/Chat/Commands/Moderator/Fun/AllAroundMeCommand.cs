#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class AllAroundMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_allaroundme";

        public string Parameters => "";

        public string Description => "Precisa de atenção? Use este comando e veja o resultado.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            var users = room.GetRoomUserManager().GetRoomUsers();
            foreach (var u in users.ToList().Where(u => u != null && session.GetHabbo().Id != u.UserId))
                u.MoveTo(user.X, user.Y, true);
        }
    }
}