namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    internal class GiftWrappingErrorComposer : ServerPacket
    {
        public GiftWrappingErrorComposer()
            : base(ServerPacketHeader.GiftWrappingErrorMessageComposer)
        {
        }
    }
}