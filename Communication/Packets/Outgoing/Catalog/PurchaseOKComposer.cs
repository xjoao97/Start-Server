#region

using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    public class PurchaseOkComposer : ServerPacket
    {
        public PurchaseOkComposer(CatalogItem item, ItemData baseItem)
            : base(ServerPacketHeader.PurchaseOkMessageComposer)
        {
            WriteInteger(baseItem.Id);
            WriteString(baseItem.ItemName);
            WriteBoolean(false);
            WriteInteger(item.CostCredits);
            WriteInteger(item.CostPixels);
            WriteInteger(0);
            WriteBoolean(true);
            WriteInteger(1);
            WriteString(baseItem.Type.ToString().ToLower());
            WriteInteger(baseItem.SpriteId);
            WriteString("");
            WriteInteger(1);
            WriteInteger(0);
            WriteString("");
            WriteInteger(1);
        }

        public PurchaseOkComposer()
            : base(ServerPacketHeader.PurchaseOkMessageComposer)
        {
            WriteInteger(0);
            WriteString("");
            WriteBoolean(false);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteBoolean(true);
            WriteInteger(1);
            WriteString("s");
            WriteInteger(0);
            WriteString("");
            WriteInteger(1);
            WriteInteger(0);
            WriteString("");
            WriteInteger(1);
        }
    }
}