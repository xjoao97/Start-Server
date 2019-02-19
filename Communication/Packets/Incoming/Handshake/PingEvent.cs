#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Handshake
{
    internal class PingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.PingCount = 0;
    }
}