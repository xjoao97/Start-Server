#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class AlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_alert_user";

        public string Parameters => "%username% %Messages%";

        public string Description => "Alerte um usuário com uma mensagem.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira o nome de usuário que você deseja alertar.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online ou não está no quarto.");
                return;
            }

            if (targetClient.GetHabbo() == null)
            {
                session.SendWhisper("O usuário não está online ou não está no quarto.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Get a life.");
                return;
            }

            var message = CommandManager.MergeParams(Params, 2);

            targetClient.SendNotification(session.GetHabbo().Username + " alertou você com a seguinte mensagem:\n\n" +
                                          message);
            session.SendWhisper("Alerta enviado com sucesso para " + targetClient.GetHabbo().Username);
        }
    }
}