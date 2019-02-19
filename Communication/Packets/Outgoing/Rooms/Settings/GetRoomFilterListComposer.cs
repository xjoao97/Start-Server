#region

using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class GetRoomFilterListComposer : ServerPacket
    {
        public GetRoomFilterListComposer(Room Instance)
            : base(ServerPacketHeader.GetRoomFilterListMessageComposer)
        {
            WriteInteger(Instance.WordFilterList.Count);
            foreach (var Word in Instance.WordFilterList)
                WriteString(Word);
        }
    }
}