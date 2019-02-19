#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableGiftsCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_gifts";

        public string Parameters => "";

        public string Description => "Você pode desativar o recebimento de presentes.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowGifts = !session.GetHabbo().AllowGifts;
            session.SendWhisper("Você " + (session.GetHabbo().AllowGifts ? "ativou" : "desativou") +
                                " o recebimento de presentes.");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_gifts` = @AllowGifts WHERE `id` = '" +
                                  session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowGifts", OblivionServer.BoolToEnum(session.GetHabbo().AllowGifts));
                dbClient.RunQuery();
            }
        }
    }
}