#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class PickTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var Junk = Packet.PopInt();
            var TicketId = Packet.PopInt();
            OblivionServer.GetGame().GetModerationManager().GetModerationTool().PickTicket(Session, TicketId);
        }
    }
}