namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class RoomInfoUpdatedComposer : ServerPacket
    {
        public RoomInfoUpdatedComposer(int roomID)
            : base(ServerPacketHeader.RoomInfoUpdatedMessageComposer)
        {
            WriteInteger(roomID);
        }
    }
}