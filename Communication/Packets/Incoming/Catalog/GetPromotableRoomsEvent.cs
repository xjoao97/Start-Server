#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class GetPromotableRoomsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Rooms = Session.GetHabbo().UsersRooms;
            Rooms =
                Rooms.Where(x => x.Promotion == null || x.Promotion.TimestampExpires < OblivionServer.GetUnixTimestamp())
                    .ToList();
            Session.SendMessage(new PromotableRoomsComposer(Rooms));
        }
    }
}