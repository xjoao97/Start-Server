#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class RemoveBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_takebadgestaff";

        public string Parameters => "%name% %idemblema%";
        public string Description => "Remova um emblema de um usuário.";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome do usuário.");
                return;
            }

            var TargetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient.GetHabbo().Rank >= 6)
                Session.SendWhisper("Você não pode remover emblema de staffs!");


            if (TargetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
            {
                TargetClient.GetHabbo().GetBadgeComponent().RemoveBadge(Params[2]);

                TargetClient.SendNotification("Um staff removeu seu emblema: " + Params[2] + "!");
                Session.SendWhisper("Você pegou o emblema: " + Params[2]);
            }

            else
            {
                Session.SendNotification("O usuário não possui esse emblema!");
            }
        }
    }
}