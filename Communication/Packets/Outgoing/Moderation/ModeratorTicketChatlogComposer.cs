#region

using System;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Support;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorTicketChatlogComposer : ServerPacket
    {
        public ModeratorTicketChatlogComposer(SupportTicket Ticket, RoomData RoomData, double Timestamp)
            : base(ServerPacketHeader.ModeratorTicketChatlogMessageComposer)
        {
            WriteInteger(Ticket.TicketId);
            WriteInteger(Ticket.SenderId);
            WriteInteger(Ticket.ReportedId);
            WriteInteger(RoomData.Id);

            WriteByte(1);
            WriteShort(2); //Count
            WriteString("roomName");
            WriteByte(2);
            WriteString(RoomData.Name);
            WriteString("roomId");
            WriteByte(1);
            WriteInteger(RoomData.Id);

            WriteShort(Ticket.ReportedChats.Count);
            foreach (var Chat in Ticket.ReportedChats)
            {
                var Habbo = OblivionServer.GetHabboById(Ticket.ReportedId);
                var time2 = ((int) OblivionServer.GetUnixTimestamp() - Convert.ToInt32(Timestamp)) * 1000;
                var time = new DateTime(time2);
                WriteString(time.ToString("hh:mm"));
                //base.WriteString(time2); 
                //base.WriteInteger(((int)OblivionServer.GetUnixTimestamp() - Convert.ToInt32(Timestamp)) * 1000);
                WriteInteger(Ticket.ReportedId);
                WriteString(Habbo != null ? Habbo.Username : "No username");
                WriteString(Chat);
                WriteBoolean(false);
            }
        }
    }
}