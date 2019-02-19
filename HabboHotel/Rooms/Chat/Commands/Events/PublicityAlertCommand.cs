/*#region

using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{ //publialert

    internal class PublicityAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_publi_alert";

        public string Parameters => "%Message%";

        public string Description => "Send a hotel alert for your event!";

        public void Execute(GameClient session, string[] Params)
        {
            OblivionServer.GetGame()
                .GetClientManager()
                .SendMessage(new SuperNotificationComposer("publi", "¡Nueva oleada en este momento!",
                    "La oleada ha sido abierta por: <b><font color='#FE2EF7'>" + Session.GetHabbo().Username +
                    " </font></b>\nAsiste a ella para ganar premios , placas y conocer nuevos amigos! ¿QUE ESPERAS?" +
                    "\r\rLas oleadas de publicidad, colaboran con el hotel para subir el contador de usuarios\n\n",
                    "Ir a la oleada", "event:navigator/goto/" + Room.Id));
        }
    }
}*/