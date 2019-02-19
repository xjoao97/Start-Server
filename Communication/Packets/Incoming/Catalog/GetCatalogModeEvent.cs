#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class GetCatalogModeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PageMode = Packet.PopString();
        }
    }
}