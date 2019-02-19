#region

using Oblivion.Communication.Packets.Outgoing.Avatar;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Avatar
{
    internal class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new WardrobeComposer(Session));
    }
}