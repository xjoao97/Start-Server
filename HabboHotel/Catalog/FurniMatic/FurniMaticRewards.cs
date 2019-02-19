using Oblivion.HabboHotel.Items;

namespace Oblivion.HabboHotel.Catalog.FurniMatic
{
    public class FurniMaticRewards
    {
        public int BaseId;
        public int DisplayId;
        public int Level;

        public FurniMaticRewards(int displayId, int baseId, int level)
        {
            DisplayId = displayId;
            BaseId = baseId;
            Level = level;
        }

        public ItemData GetBaseItem() => OblivionServer.GetGame().GetItemManager().GetItem(BaseId);
    }
}