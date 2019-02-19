#region

using Oblivion.HabboHotel.Moderation;
using Oblivion.HabboHotel.Support;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorSupportTicketComposer : ServerPacket
    {
        public ModeratorSupportTicketComposer(SupportTicket Ticket)
            : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
        {
            WriteInteger(Ticket.Id);
            WriteInteger(Ticket.TabId);
            WriteInteger(Ticket.Type); // Type
            WriteInteger(Ticket.Category); // Category
            WriteInteger(((int) OblivionServer.GetUnixTimestamp() - (int) Ticket.Timestamp) * 1000);
            WriteInteger(Ticket.Score);
            WriteInteger(0);
            WriteInteger(Ticket.SenderId);
            WriteString(Ticket.SenderName);
            WriteInteger(Ticket.ReportedId);
            WriteString(Ticket.ReportedName);
            WriteInteger(Ticket.Status == ModerationTicket.TicketStatus.PICKED ? Ticket.ModeratorId : 0);
            WriteString(Ticket.ModName);
            WriteString(Ticket.Message);
            WriteInteger(0); //No idea?
            WriteInteger(0); //String, int, int - this is the "matched to" a string
            {
                WriteString("habbons.com");
                WriteInteger(-1);
                WriteInteger(-1);
            }
        }
    }
}