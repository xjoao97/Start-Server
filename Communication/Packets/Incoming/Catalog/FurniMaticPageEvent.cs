using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class FurniMaticPageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null) return;
            var response = new ServerPacket(ServerPacketHeader.FurniMaticNoRoomError);
            response.WriteInteger(1);
            response.WriteInteger(0);
            Session.SendMessage(response);
        }
    }
}