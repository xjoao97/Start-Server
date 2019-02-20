#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class UnmuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_unmute";

        public string Parameters => "%username%";

        public string Description => "Ative a fala de um usuário silenciado.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o nome do usuário que foi silenciado.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null || targetClient.GetHabbo() == null)
            {
                session.SendWhisper("Talvez o usuário não esteja online.");
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("UPDATE `users` SET `time_muted` = '0' WHERE `id` = '" + targetClient.GetHabbo().Id +
                                  "' LIMIT 1");
            }

            targetClient.GetHabbo().TimeMuted = 0;
            targetClient.SendNotification("Você pode falar, graças á: " + session.GetHabbo().Username + "!");
            session.SendWhisper("Você permitiu que " + targetClient.GetHabbo().Username + " falasse novamente!");
        }
    }
}