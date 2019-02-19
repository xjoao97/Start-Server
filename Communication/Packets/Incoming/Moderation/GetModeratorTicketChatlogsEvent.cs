#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorTicketChatlogsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_tickets"))
                return;

            var Ticket = OblivionServer.GetGame().GetModerationManager().GetModerationTool().GetTicket(Packet.PopInt());
            if (Ticket == null)
                return;

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Ticket.RoomId);
            if (Data == null)
                return;

            Session.SendMessage(new ModeratorTicketChatlogComposer(Ticket, Data, Ticket.Timestamp));
        }
    }
}