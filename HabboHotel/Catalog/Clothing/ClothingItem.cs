#region

using System.Collections.Generic;

#endregion

namespace Oblivion.HabboHotel.Catalog.Clothing
{
    public class ClothingItem
    {
        public ClothingItem(int Id, string ClothingName, string PartIds)
        {
            this.Id = Id;
            this.ClothingName = ClothingName;

            this.PartIds = new List<int>();
            if (PartIds.Contains(","))
                foreach (var PartId in PartIds.Split(','))
                    this.PartIds.Add(int.Parse(PartId));
            else if (!string.IsNullOrEmpty(PartIds) && int.Parse(PartIds) > 0)
                this.PartIds.Add(int.Parse(PartIds));
        }

        public int Id { get; set; }
        public string ClothingName { get; set; }
        public List<int> PartIds { get; }
    }
}