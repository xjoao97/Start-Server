#region

using System.Collections.Generic;
using System.Data;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class EventAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_event_alert";

        public string Parameters => "%id%";

        public string Description => Language.GetValue("eha.desc");

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params.Length <= 1)
                return;

            int id;
            int.TryParse(Params[1], out id);
            if (id <= 0)
            {
                Session.SendWhisper(Language.GetValue("eha.invalidid"));
                return;
            }
            DataRow row;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT name FROM eventlist WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", id);
                row = dbClient.GetRow();
            }
            if (row == null)
            {
                Session.SendWhisper(Language.GetValue("eha.notfound"));
                return;
            }
            var list = new List<string>
            {
                $"<font color=\"#006400\">{Session.GetHabbo().Username}</font>",
                "<font color=\"#0F89CF\">" + row["name"] + "</font>",
                "color=\"#e70000\""
            };

            OblivionServer.GetGame()
                .GetClientManager()
                .EventAlert(new RoomNotificationComposer(Language.GetValue("eha.title"),
                    Language.GetValueWithVar("eha.text", list),
                    "eventoimage", Language.GetValue("eha.button"),
                    "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
        }
    }
}