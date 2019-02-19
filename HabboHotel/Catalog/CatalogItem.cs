#region

using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Catalog
{
    public class CatalogItem
    {
        public CatalogItem(int Id, int ItemId, ItemData Data, string CatalogName, int PageId, int CostCredits,
            int CostPixels,
            int CostDiamonds, int CostGotw, int Amount, int LimitedEditionSells, int LimitedEditionStack, bool HaveOffer,
            string ExtraData, string Badge, int OfferId)
        {
            this.Id = Id;
            Name = CatalogName;
            this.ItemId = ItemId;
            this.Data = Data;
            PageID = PageId;
            this.CostCredits = CostCredits;
            this.CostPixels = CostPixels;
            this.CostDiamonds = CostDiamonds;
            this.CostGotw = CostGotw;
            this.Amount = Amount;
            this.LimitedEditionSells = LimitedEditionSells;
            this.LimitedEditionStack = LimitedEditionStack;
            IsLimited = LimitedEditionStack > 0;
            this.HaveOffer = HaveOffer;
            this.ExtraData = ExtraData;
            this.Badge = Badge;
            this.OfferId = OfferId;
        }

        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemData Data { get; set; }
        public int Amount { get; set; }
        public int CostCredits { get; set; }
        public string ExtraData { get; set; }
        public bool HaveOffer { get; set; }
        public bool IsLimited { get; set; }
        public string Name { get; set; }
        public int PageID { get; set; }
        public int CostPixels { get; set; }
        public int LimitedEditionStack { get; set; }
        public int LimitedEditionSells { get; set; }
        public int CostDiamonds { get; set; }
        public int CostGotw { get; set; }
        public string Badge { get; set; }
        public int OfferId { get; set; }
    }
}