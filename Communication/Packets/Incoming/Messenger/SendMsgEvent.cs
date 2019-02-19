#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class SendMsgEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var userId = Packet.PopInt();
            var message = StringCharFilter.Escape(Packet.PopString());

            if (Session.GetHabbo().Rank < 8 &&
                OblivionServer.GetGame().GetChatManager().GetFilter().IsFiltered(message))            {
                OblivionServer.GetGame()
                    .GetClientManager()
                    .StaffAlert(new RoomNotificationComposer("Alerta de Divulgação",
                        "O usuário: <b>" + Session.GetHabbo().Username + "<br>" +
                        "<br></b> Está dizendo uma palavra que foi bloqueada no filtro " + "<br>" +
                        "<br></b> Onde foi usado: <b> Chat por Console <br>" +
                        "<br><b>A palavra foi:</b><br>" +
                        "<br>" + "<b>" + "<font color =\"#06087F\">" + message + "</font>" + "</b><br>" +
                        "<br>Para ir na sala, clique em \"Ir à Sala \"</b>",
                        "filter", "Ir à Sala", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                message = "Estou tentando publicar um Hotel, chame um staff para me banir.";
            }


            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Opa, parece que você está mudo.");
                return;
            }

            if (userId < 0)
            {
                Group group = (OblivionServer.GetGame().GetGroupManager().TryGetGroup(-userId));
                if (group != null)
                {
                    if (!group.IsMember(Session.GetHabbo().Id) || !group.HasChat) return;
                    if (!group.ChatUsers.Contains(Session.GetHabbo().Id))
                    {
                        Session.SendNotification(
                            "Você desativou as mensagens deste grupo, para ativar novamente digite :groupnotif enable.");

                        return;
                    }
                    Session.GetHabbo().GetMessenger().SendGroupMessage(message, group);
                    return;
                }
            }

            Session.GetHabbo().GetMessenger().SendInstantMessage(userId, message);
        }
    }
}