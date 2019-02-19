#region

using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemUpdateComposer : ServerPacket
    {
        public ItemUpdateComposer(Item Item, int UserId)
            : base(ServerPacketHeader.ItemUpdateMessageComposer)
        {
            if (Item == null || UserId <= 0)
                return;

            WriteWallItem(Item, UserId);
        }

        private void WriteWallItem(Item Item, int UserId)
        {
            if (Item == null || UserId <= 0)
                return;

            WriteString(Item.Id.ToString());
            WriteInteger(Item.GetBaseItem().SpriteId);
            WriteString(Item.wallCoord);
            switch (Item.GetBaseItem().InteractionType)
            {
                case InteractionType.Postit:
                    WriteString(Item.ExtraData.Split(' ')[0]);
                    break;

                default:
                    WriteString(Item.ExtraData);
                    break;
            }
            WriteInteger(-1);
            WriteInteger(Item.GetBaseItem().Modes > 1 ? 1 : 0);
            WriteInteger(UserId);
        }
    }
}