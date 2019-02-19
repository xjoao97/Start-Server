namespace Oblivion.Communication.Packets.Outgoing.Rooms.Camera
{
    public class CameraPriceComposer : ServerPacket
    {
        public CameraPriceComposer(int purchasePriceCoins, int purchasePriceDuckets, int publishPriceDuckets)
            : base(ServerPacketHeader.CameraPriceComposer)
        {
            WriteInteger(purchasePriceCoins);
            WriteInteger(purchasePriceDuckets);
            WriteInteger(publishPriceDuckets);
        }
    }
}