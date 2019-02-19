#region

using System.Data;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class EventListCommand : IChatCommand
    {
        public string PermissionRequired => "command_event_alert";

        public string Parameters => "%message%";

        public string Description => "Enviar um alerta de evento para seu Hotel!";

        public void Execute(GameClient session, string[] Params)
        {

            DataTable events;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`name` FROM `eventlist`");
                events = dbClient.getTable();
            }
            var builder = new StringBuilder();
            foreach (DataRow row in events.Rows) //pega cada valor da sql
                builder.Append(row["id"] + " - " + row["name"] + "\r");

            session.SendMessage(new MotdNotificationComposer(builder.ToString()));
        }
    }
}