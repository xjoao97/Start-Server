#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class MoonwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_moonwalk";

        public string Parameters => "";

        public string Description => "Hey, você quer imitar o passo do Michael Jackson? Use este comando.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.moonwalkEnabled = !user.moonwalkEnabled;

            session.SendWhisper(user.moonwalkEnabled ? "Moonwalk ativado!" : "Moonwalk desativado!");
        }
    }
}