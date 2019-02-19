/*#region

using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{ //da2alert

    internal class DiceAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_da2_alert";

        public string Parameters => "%Message%";

        public string Description => "Send a hotel alert for your event!";

        public void Execute(GameClient session, string[] Params)
        {

            OblivionServer.GetGame()
                .GetClientManager()
                .SendMessage(new SuperNotificationComposer("da2alert", "¡Se han abierto los dados oficiales!",
                    "El inter que abre los dados es: <b><font color='#FF8000'>" + session.GetHabbo().Username +
                    " </font></b>\nA diferencia de los dados comunes, es que en estos puedes apostar con total seguridad" +
                    "\r\rLos inters serán los encargados de supervisar que todo se realiza de manera correcta\n\n ¡¿A QUE ESPERAS?! ¡Ven ya y gana apostando contra otros usuarios!",
                    "Ir a la sala", "event:navigator/goto/" + Room.Id));
        }
    }
}*/