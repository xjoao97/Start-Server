#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class CloseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var Result = Packet.PopInt(); // result, 1 = useless, 2 = abusive, 3 = resolved
            var Junk = Packet.PopInt(); // ? 
            var TicketId = Packet.PopInt(); // id

            OblivionServer.GetGame().GetModerationManager().GetModerationTool().CloseTicket(Session, TicketId, Result);
        }
    }
}