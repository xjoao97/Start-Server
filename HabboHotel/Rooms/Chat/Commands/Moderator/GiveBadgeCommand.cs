#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GiveBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_give_badge";

        public string Parameters => "%username% %badge%";

        public string Description => "Envie um emblema a algum usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length != 3)
            {
                session.SendWhisper(
                    "Por favor insira um nome de usuário e o código do emblema que você gostaria de dar!");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient != null)
                if (!targetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                {
                    targetClient.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, targetClient);
                    if (targetClient.GetHabbo().Id != session.GetHabbo().Id)
                        targetClient.SendMessage(new RoomNotificationComposer("" , "Você acaba de receber o emblema (" + Params[2] + ")!"  , "generic", "", "", true));
                    else
                        session.SendWhisper("Emblema enviado com sucesso " + Params[2] + "!");
                }
                else
                {
                    session.SendWhisper("Opa, esse usuário já possui este emblema (" + Params[2] + ") !");
                }
            else
                session.SendWhisper("Opa, não foi possível encontrar esse usuário!");
        }
    }
}