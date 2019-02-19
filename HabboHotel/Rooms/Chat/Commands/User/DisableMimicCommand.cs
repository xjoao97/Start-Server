#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableMimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_mimic";

        public string Parameters => "";

        public string Description => "Não quer que copiem seu visual? Use esse comando.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowMimic = !session.GetHabbo().AllowMimic;
            session.SendWhisper("Você " + (session.GetHabbo().AllowMimic ? "ativou" : "desativou") +
                                " o uso do comando copiar em você.");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_mimic` = @AllowMimic WHERE `id` = '" +
                                  session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowMimic", OblivionServer.BoolToEnum(session.GetHabbo().AllowMimic));
                dbClient.RunQuery();
            }
        }
    }
}