namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class HelperSessionVisiteRoomComposer : ServerPacket
    {
        public HelperSessionVisiteRoomComposer(int roomId)
            : base(ServerPacketHeader.HelperSessionVisiteRoomMessageComposer)
        {
            WriteInteger(roomId);
        }
    }
}