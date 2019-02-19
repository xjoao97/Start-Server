#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SpinCommand : IChatCommand
    {
        public string PermissionRequired => "command_spin";

        public string Parameters => "";

        public string Description => "Girando...";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;
            user.ApplyEffect(500);
            session.SendWhisper("Girando, girando e girando. ;)");
        }
    }
}