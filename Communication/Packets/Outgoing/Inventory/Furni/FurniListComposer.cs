#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items;

#endregion
namespace Oblivion.Communication.Packets.Outgoing.Inventory.Furni
{
    internal class FurniListComposer : ServerPacket
    {
        public FurniListComposer(List<Item> Items)
            : base(ServerPacketHeader.FurniListMessageComposer)
        {
            WriteInteger(1);
            WriteInteger(1);

            WriteInteger(Items.Count);
            foreach (var Item in Items.ToList())
                WriteItem(Item);
            
        }

        private void WriteItem(Item Item)
        {
            WriteInteger(Item.Id);
            WriteString(Item.GetBaseItem().Type.ToString().ToUpper());
            WriteInteger(Item.Id);
            WriteInteger(Item.GetBaseItem().SpriteId);

            if (Item.LimitedNo > 0)
            {
                WriteInteger(1);
                WriteInteger(256);
                WriteString(Item.ExtraData);
                WriteInteger(Item.LimitedNo);
                WriteInteger(Item.LimitedTot);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(Item, this);
            }

            WriteBoolean(Item.GetBaseItem().AllowEcotronRecycle);
            WriteBoolean(Item.GetBaseItem().AllowTrade);
            WriteBoolean(Item.LimitedNo == 0 && Item.GetBaseItem().AllowInventoryStack);
            WriteBoolean(true);
            WriteInteger(-1); //Seconds to expiration.
            WriteBoolean(false);
            WriteInteger(-1); //Item RoomId

            if (!Item.IsWallItem)
            {
                WriteString(string.Empty);
                WriteInteger(0);
            }
        }
    }
}