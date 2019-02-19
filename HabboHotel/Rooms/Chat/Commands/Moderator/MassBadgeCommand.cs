#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MassBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_mass_badge";

        public string Parameters => "%badge%";

        public string Description => "Dê um emblema para todo o hotel.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o código do emblema que você deseja enviar.");
                return;
            }

            foreach (
                var client in
                OblivionServer.GetGame()
                    .GetClientManager()
                    .GetClients.ToList()
                    .Where(
                        client =>
                            client?.GetHabbo() != null && client.GetHabbo().Username != session.GetHabbo().Username))
                if (!client.GetHabbo().GetBadgeComponent().HasBadge(Params[1]))
                {
                    client.GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, client);
                    client.SendNotification("Você recebeu um emblema!");
                }
                else
                {
                    client.SendWhisper(session.GetHabbo().Username +
                                       " tentou lhe dar um emblema, mas parece que você já o tem!");
                }

            session.SendWhisper("Você enviou o emblema para todo o Hotel.");
        }
    }
}