#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class KickCommand : IChatCommand
    {
        public string PermissionRequired => "command_kick";

        public string Parameters => "%username% %reason%";

        public string Description => "Expulse um usuário do quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o nome do usuário que você deseja expulsar.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Ocorreu um erro, o usuário não está online.");
                return;
            }

            if (targetClient.GetHabbo() == null)
            {
                session.SendWhisper("Ocorreu um erro, o usuário não está online.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Quer realmente se expulsar? Acho que não.");
                return;
            }

            if (!targetClient.GetHabbo().InRoom)
            {
                session.SendWhisper("O usuário não está no quarto.");
                return;
            }

            Room targetRoom;
            if (
                !OblivionServer.GetGame()
                    .GetRoomManager()
                    .TryGetRoom(targetClient.GetHabbo().CurrentRoomId, out targetRoom))
                return;

            if (Params.Length > 2)
                targetClient.SendNotification("Um moderador expulsou você com a seguinte razão: " +
                                              CommandManager.MergeParams(Params, 2));
            else
                targetClient.SendNotification("Um moderador lhe expulsou do quarto.");

            targetRoom.GetRoomUserManager().RemoveUserFromRoom(targetClient, true);
        }
    }
}