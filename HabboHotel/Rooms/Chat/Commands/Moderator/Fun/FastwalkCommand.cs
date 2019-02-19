#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class FastwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_fastwalk";

        public string Parameters => "";

        public string Description => "Te dá a habilidade de correr rápido.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.FastWalking = !user.FastWalking;

            if (user.SuperFastWalking)
                user.SuperFastWalking = false;

            session.SendWhisper("Feito.");
        }
    }
}