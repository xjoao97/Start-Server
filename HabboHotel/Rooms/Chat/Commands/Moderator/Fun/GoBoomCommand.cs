#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class GoBoomCommand : IChatCommand
    {
        public string PermissionRequired => "command_goboom";

        public string Parameters => "";

        public string Description => "Make the entire room go boom! (Applys effect 108)";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var users = room.GetRoomUserManager().GetRoomUsers();
            if (users.Count > 0)
                foreach (var u in users.ToList().Where(u => u != null))
                    u.ApplyEffect(108);
        }
    }
}