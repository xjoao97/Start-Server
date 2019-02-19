#region



#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Furni
{
    internal class UpdateMagicTileComposer : ServerPacket
    {
        public UpdateMagicTileComposer(int ItemId, int Decimal)
            : base(ServerPacketHeader.UpdateMagicTileMessageComposer)
        {
            WriteInteger(ItemId);
            WriteInteger(Decimal);
        }
    }
}