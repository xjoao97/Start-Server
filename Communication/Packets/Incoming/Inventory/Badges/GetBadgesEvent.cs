#region

using Oblivion.Communication.Packets.Outgoing.Inventory.Badges;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Badges
{
    internal class GetBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new BadgesComposer(Session));
    }
}