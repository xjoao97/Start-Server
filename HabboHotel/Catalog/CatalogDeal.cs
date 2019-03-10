#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Catalog
{
    public class CatalogDeal
    {
        public CatalogDeal(int Id, int PageId, string Items, string DisplayName, int Credits, int Pixels,
            ItemDataManager ItemDataManager)
        {
            this.Id = Id;
            this.PageId = PageId;
            this.DisplayName = DisplayName;
            ItemDataList = new List<CatalogItem>();

            var SplitItems = Items.Split(';');
            foreach (var Item in SplitItems.Select(Split => Split.Split('*')))
            {
                if (!int.TryParse(Item[0], out int ItemId) || !int.TryParse(Item[1], out int Amount))
                {
                    continue;
                }

                ItemData Data = OblivionServer.GetGame().GetItemManager().GetItem(ItemId);
                if (Data == null)
                    continue;

                ItemDataList.Add(new CatalogItem(0, ItemId, Data, string.Empty, PageId, 0, 0, 0, 0, 0, 0, 0, false, "",
                    "", 0));
            }

            CostCredits = Credits;
            CostPixels = Pixels;
        }

        public int Id { get; set; }
        public int PageId { get; set; }
        public List<CatalogItem> ItemDataList { get; }
        public string DisplayName { get; set; }
        public int CostCredits { get; set; }
        public int CostPixels { get; set; }
    }
}