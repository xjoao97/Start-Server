namespace Oblivion.Communication.Packets.Outgoing.Handshake
{
    internal class AvailabilityStatusComposer : ServerPacket
    {
        public AvailabilityStatusComposer()
            : base(ServerPacketHeader.AvailabilityStatusMessageComposer)
        {
            //  base.WriteBoolean(true);

            WriteBoolean(true);
            WriteBoolean(false);
            WriteBoolean(true);
        }
    }
}