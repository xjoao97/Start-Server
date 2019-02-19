#region

using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Avatar
{
    internal class DanceComposer : ServerPacket
    {
        public DanceComposer(RoomUser Avatar, int Dance)
            : base(ServerPacketHeader.DanceMessageComposer)
        {
            WriteInteger(Avatar.VirtualId);
            WriteInteger(Dance);
        }
    }
}