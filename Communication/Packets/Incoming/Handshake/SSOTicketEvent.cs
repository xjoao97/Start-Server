#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Handshake
{
    public class SSOTicketEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ticket = Packet.PopString();
            if (Session.GetHabbo() != null || ticket == null)
                return;

            Session.TryAuthenticate(ticket);
        }
    }
}