namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class HelperSessionInvinteRoomComposer : ServerPacket
    {
        public HelperSessionInvinteRoomComposer(int int1, string str)
            : base(ServerPacketHeader.HelperSessionInvinteRoomMessageComposer)
        {
            WriteInteger(int1);
            WriteString(str);
        }
    }
}