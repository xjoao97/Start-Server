#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_badge";

        public string Parameters => "%badge%";

        public string Description => "Envie um emblema para todo o quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, indique o nome do emblema que você gostaria de dar para o quarto.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            foreach (
                var user in
                room.GetRoomUserManager()
                    .GetUserList()
                    .ToList()
                    .Where(user => user?.GetClient() != null && user.GetClient().GetHabbo() != null))
                if (!user.GetClient().GetHabbo().GetBadgeComponent().HasBadge(Params[1]))
                {
                    user.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, user.GetClient());
                    user.GetClient().SendNotification("Você acaba de receber um emblema.");
                }
                else
                {
                    user.GetClient()
                        .SendWhisper(session.GetHabbo().Username +
                                     " tentou lhe mandar um emblema, mas parece que você já o tem.");
                }

            session.SendWhisper("Você enviou um emblema para toda a sala.");
        }
    }
}