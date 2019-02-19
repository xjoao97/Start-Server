#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class SendRoomInviteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Oops, you're currently muted - you cannot send room invitations.");
                return;
            }

            var Amount = Packet.PopInt();
            if (Amount > 500)
                return; // don't send at all

            var Targets = new List<int>();
            for (var i = 0; i < Amount; i++)
            {
                var uid = Packet.PopInt();
                if (i < 100) // limit to 100 people, keep looping until we fulfil the request though
                    Targets.Add(uid);
            }

            var Message = StringCharFilter.Escape(Packet.PopString());
            if (OblivionServer.GetGame().GetChatManager().GetFilter().IsFiltered(Message))
            {
                OblivionServer.GetGame()
                    .GetClientManager()
                    .StaffAlert(new RoomNotificationComposer("Alerta de Publicidad",
                        "El Usuario: <b>" + Session.GetHabbo().Username + "<br>" +
                        "<br></b> Esta Publicando y/o usando una palabra contenida en el filtro " + "<br>" +
                        "<br></b> Mediante : <b> Invitación de Chat Por Consola <br>" +
                        "<br><b>La Palabra Usada Fue:</b><br>" +
                        "<br>" + "<b>" + "<font color =\"#06087F\">" + Message + "</font>" + "</b><br>" +
                        "<br>Para ir a la sala, da clic en \"Ir a la Sala \"</b>",
                        "filter", "Ir a la Sala", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                Message = "Estoy Intentando Publicar Otro Hotel Porfavor Advierte a un Staff";
            }
            if (Message.Length > 121)
                Message = Message.Substring(0, 121);

            foreach (
                var Client in
                Targets.Where(UserId => Session.GetHabbo().GetMessenger().FriendshipExists(UserId))
                    .Select(UserId => OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId))
                    .Where(
                        Client =>
                            Client != null && Client.GetHabbo() != null &&
                            !Client.GetHabbo().AllowMessengerInvites && Client.GetHabbo().AllowConsoleMessages))
                Client.SendMessage(new RoomInviteComposer(Session.GetHabbo().Id, Message));

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `chatlogs_console_invitations` (`user_id`,`message`,`timestamp`) VALUES ('" +
                    Session.GetHabbo().Id + "', @message, UNIX_TIMESTAMP())");
                dbClient.AddParameter("message", Message);
                dbClient.RunQuery();
            }
        }
    }
}