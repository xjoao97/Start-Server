namespace Oblivion.Communication.Packets.Outgoing.BuildersClub
{
    internal class BCBorrowedItemsComposer : ServerPacket
    {
        public BCBorrowedItemsComposer()
            : base(ServerPacketHeader.BcBorrowedItemsMessageComposer)
        {
            WriteInteger(0);
        }
    }
}