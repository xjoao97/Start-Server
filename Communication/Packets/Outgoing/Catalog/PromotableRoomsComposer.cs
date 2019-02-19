#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    internal class PromotableRoomsComposer : ServerPacket
    {
        public PromotableRoomsComposer(ICollection<RoomData> Rooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            WriteBoolean(true);
            WriteInteger(Rooms.Count); //Count

            foreach (var Data in Rooms)
            {
                WriteInteger(Data.Id);
                WriteString(Data.Name);
                WriteBoolean(false);
            }
        }
    }
}