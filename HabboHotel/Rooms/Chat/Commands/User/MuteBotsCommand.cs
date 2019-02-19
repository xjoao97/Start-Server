#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class MuteBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_bots";

        public string Parameters => "";

        public string Description => "Ignore as conversas dos bots";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowBotSpeech = !session.GetHabbo().AllowBotSpeech;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `bots_muted` = '" + (session.GetHabbo().AllowBotSpeech ? 1 : 0) +
                                  "' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            session.SendWhisper(session.GetHabbo().AllowBotSpeech
                ? "Sucesso, agora você não vê mais as conversas de bots."
                : "Opa, agora você já pode ver as conversas dos bots.");
        }
    }
}