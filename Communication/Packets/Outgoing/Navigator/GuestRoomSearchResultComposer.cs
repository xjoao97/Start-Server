#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class GuestRoomSearchResultComposer : ServerPacket
    {
        public GuestRoomSearchResultComposer(int Mode, string UserQuery, ICollection<RoomData> Rooms)
            : base(ServerPacketHeader.GuestRoomSearchResultMessageComposer)
        {
            WriteInteger(Mode);
            WriteString(UserQuery);

            WriteInteger(Rooms.Count);
            foreach (var data in Rooms)
                RoomAppender.WriteRoom(this, data, data.Promotion);

            WriteBoolean(false);
        }
    }
}