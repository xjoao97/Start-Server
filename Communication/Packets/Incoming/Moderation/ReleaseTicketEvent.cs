#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ReleaseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var Amount = Packet.PopInt();
            for (var i = 0; i < Amount; i++)
            {
                var TicketId = Packet.PopInt();
                OblivionServer.GetGame().GetModerationManager().GetModerationTool().ReleaseTicket(Session, TicketId);
            }
        }
    }
}