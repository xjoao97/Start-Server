#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Misc
{
    internal class EventTrackerEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
        }
    }
}