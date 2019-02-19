#region

using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class FlagMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_flagme";

        public string Parameters => "";

        public string Description => "Quer mudar seu nickname? Utilize este comando.";

        public void Execute(GameClient session, string[] Params)
        {
            if (!CanChangeName(session.GetHabbo()))
            {
                session.SendWhisper("Desculpe, parece que você atualmente não tem a opção de mudar seu nome de usuário!");
                return;
            }

            session.GetHabbo().ChangingName = true;
            session.SendNotification(
                "Por favor, esteja ciente de que, se o seu nome de usuário for considerado como impróprio, você será banido sem dúvida.\r\rObserve também que os STAFFS não irão mudar seu nome de usuário novamente, cuidado.\r\rFeche esta notificação e clique no seu personagem para alterar seu nickname!");
            session.SendMessage(new UserObjectComposer(session.GetHabbo()));
        }

        private static bool CanChangeName(Habbo habbo)
        {
            if (habbo.Rank > 1 && habbo.LastNameChange == 0)
                return true;
            return habbo.GetPermissions().HasRight("mod_tool");
        }
    }
}