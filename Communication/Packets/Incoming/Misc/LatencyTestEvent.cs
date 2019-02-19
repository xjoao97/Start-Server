#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Misc
{
    internal class LatencyTestEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //Session.SendMessage(new LatencyTestComposer(Packet.PopInt()));
        }
    }
}