namespace Oblivion.Communication.Packets.Outgoing.Misc
{
    internal class LatencyTestComposer : ServerPacket
    {
        public LatencyTestComposer(int testResponce)
            : base(ServerPacketHeader.LatencyResponseMessageComposer)
        {
            WriteInteger(testResponce);
        }
    }
}