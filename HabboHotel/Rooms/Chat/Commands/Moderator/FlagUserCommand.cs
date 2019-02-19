#region

using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class FlagUserCommand : IChatCommand
    {
        public string PermissionRequired => "command_flaguser";

        public string Parameters => "%username%";

        public string Description => "Force um usuário a trocar de nome de usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o nome de usuário que deseja mudar.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez eles não estão online.");
                return;
            }

            if (targetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("You are not allowed to flag that user.");
            }
            else
            {
                targetClient.GetHabbo().LastNameChange = 0;
                targetClient.GetHabbo().ChangingName = true;
                targetClient.SendNotification(
                    "Por favor, esteja ciente de que, se o seu nome de usuário é considerado como impróprio, você será banido sem dúvida.\r\rObserve também que o pessoal não vai permitir que você altere seu nome de usuário novamente, você deve ter um problema com o que você escolheu.\r\rFechar esta janela e clique em si mesmo para começar a escolher um novo nome de usuário!");
                targetClient.SendMessage(new UserObjectComposer(targetClient.GetHabbo()));
            }
        }
    }
}