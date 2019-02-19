#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableEvent : IChatCommand
    {
        public string PermissionRequired => "command_sit";

        public string Parameters => "";

        public string Description => "Desative ou reative o alerta de eventos";

        public void Execute(GameClient Session, string[] Params)
        {
            Session.GetHabbo().DisableEventAlert = !Session.GetHabbo().DisableEventAlert;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                var Disable = OblivionServer.BoolToEnum(Session.GetHabbo().DisableEventAlert);
                dbClient.SetQuery("UPDATE users SET disabled_alert = '" + Disable + "' WHERE id = '" +
                                  Session.GetHabbo().Id + "'");
                dbClient.RunQuery();
            }
            Session.SendWhisper(
                "O alerta de eventos foi " + (Session.GetHabbo().DisableEventAlert ? "desativado" : "ativado"));
        }
    }
}