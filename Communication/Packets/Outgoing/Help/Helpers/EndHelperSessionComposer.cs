namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class EndHelperSessionComposer : ServerPacket
    {
        public EndHelperSessionComposer(int closeCode = 0)
            : base(ServerPacketHeader.EndHelperSessionMessageComposer)
        {
            WriteInteger(closeCode);
        }
    }
}