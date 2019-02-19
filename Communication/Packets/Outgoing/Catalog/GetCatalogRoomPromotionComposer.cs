#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    internal class GetCatalogRoomPromotionComposer : ServerPacket
    {
        public GetCatalogRoomPromotionComposer(List<RoomData> UsersRooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            WriteBoolean(true); //wat
            WriteInteger(UsersRooms.Count); //Count of rooms?
            foreach (var Room in UsersRooms)
            {
                WriteInteger(Room.Id);
                WriteString(Room.Name);
                WriteBoolean(true);
            }
        }
    }
}