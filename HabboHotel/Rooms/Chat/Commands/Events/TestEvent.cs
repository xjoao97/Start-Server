/*#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class TesteEventAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_event_alert";

        public string Parameters => "%message%";

        public string Description => "Enviar um alerta de evento para seu Hotel!";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, digite uma mensagem para enviar.");
                return;
            }
            var message = CommandManager.MergeParams(Params, 1);
            OblivionServer.GetGame()
                .GetClientManager()
                .EventAlert(new RoomNotificationComposer("Está acontecendo um evento!",
                    "Há um novo evento acontecendo agora, se você quer ganhar prêmios corra para o evento." +
                    "<br>" +
                    "<br>É um evento realizado pelo staff: <b> " + "<font color =\"#006400\">" +
                    Session.GetHabbo().Username + "</font>" + "<br>" +
                    "</b>Participe e ganhe o evento." +
                    "<br>" +
                    "<br>E o evento é: <br>" +
                    "<br>" + "<b>" + "<font color =\"#0F89CF\">" + message + "</font>" + "</b><br>" +
                    "<br>Venha rápido, antes que alguém tome seu lugar. <font color =\"#e70000\">Quer bloquear esses anúncios? Digite :eventosoff</font> </b>",
                    "event_image", "Ir ao evento",
                    "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
        }
    }
}*/