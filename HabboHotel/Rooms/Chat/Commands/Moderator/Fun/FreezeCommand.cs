#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class FreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_freeze";

        public string Parameters => "%username%";

        public string Description => "Congele um usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, escolha um usuário.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Talvez o usuário não esteja online.");
                return;
            }

            var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (targetUser != null)
                targetUser.Frozen = true;

            session.SendWhisper("Sucesso, o usuário " + targetClient.GetHabbo().Username + "está congelado!");
        }
    }
}