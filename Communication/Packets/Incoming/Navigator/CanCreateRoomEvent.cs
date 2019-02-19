#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class CanCreateRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            => Session.SendMessage(new CanCreateRoomComposer(false, 150));
    }
}