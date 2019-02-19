#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Moderation;
using Oblivion.HabboHotel.Support;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorInitComposer : ServerPacket
    {
        public ModeratorInitComposer(ICollection<string> UserPresets, ICollection<string> RoomPresets,
            Dictionary<string, List<ModerationPresetActionMessages>> UserActionPresets,
            ICollection<SupportTicket> Tickets)
            : base(ServerPacketHeader.ModeratorInitMessageComposer)
        {
            WriteInteger(Tickets.Count);
            foreach (var ticket in Tickets.ToList())
            {
                WriteInteger(ticket.Id);
                WriteInteger(ticket.TabId);
                WriteInteger(1); // Type
                WriteInteger(ticket.Category); // Category
                WriteInteger(((int) OblivionServer.GetUnixTimestamp() - Convert.ToInt32(ticket.Timestamp)) * 1000);
                WriteInteger(ticket.Score);
                WriteInteger(0);
                WriteInteger(ticket.SenderId);
                WriteString(ticket.SenderName);
                WriteInteger(ticket.ReportedId);
                WriteString(ticket.ReportedName);
                WriteInteger(ticket.Status == ModerationTicket.TicketStatus.PICKED ? ticket.ModeratorId : 0);
                WriteString(ticket.ModName);
                WriteString(ticket.Message);
                WriteInteger(0);
                WriteInteger(0);
            }


            WriteInteger(UserPresets.Count);
            foreach (var pre in UserPresets)
                WriteString(pre);


            WriteInteger(UserActionPresets.Count);
            foreach (var Cat in UserActionPresets.ToList())
                WriteString(Cat.Key);


            WriteBoolean(true); // Ticket right
            WriteBoolean(true); // Chatlogs
            WriteBoolean(true); // User actions alert etc
            WriteBoolean(true); // Kick users
            WriteBoolean(true); // Ban users
            WriteBoolean(true); // Caution etc
            WriteBoolean(true); // Love you, Tom


            WriteInteger(RoomPresets.Count);
            foreach (var pre in RoomPresets)
                WriteString(pre);
        }
    }
}