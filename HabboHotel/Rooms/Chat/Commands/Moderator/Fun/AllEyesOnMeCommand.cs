#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Pathfinding;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class AllEyesOnMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_alleyesonme";

        public string Parameters => "";

        public string Description => "Precisa de atenção? Use este comando.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            var users = room.GetRoomUserManager().GetRoomUsers();
            foreach (var u in users.ToList().Where(u => u != null && session.GetHabbo().Id != u.UserId))
                u.SetRot(Rotation.Calculate(u.X, u.Y, thisUser.X, thisUser.Y), false);
        }
    }
}