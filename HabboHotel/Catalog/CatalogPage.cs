#region

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Catalog
{
    public class CatalogPage
    {
//todo: remove slit('|')... (this method use more ram..)
        public CatalogPage(int Id, int ParentId, string Enabled, string Caption, string PageLink, int Icon, int MinRank,
            int MinVIP,
            string Visible, string Template, string PageStrings1, string PageStrings2,
            HybridDictionary Items, Dictionary<int, CatalogDeal> Deals, ref HybridDictionary flatOffers)
        {
            this.Id = Id;
            this.ParentId = ParentId;
            this.Enabled = Enabled == "1";
            this.Caption = Caption;
            this.PageLink = PageLink;
            this.Icon = Icon;
            MinimumRank = MinRank;
            this.Visible = Visible == "1";
            this.Template = Template;

            foreach (var Str in PageStrings1.Split('|'))
            {
                if (this.PageStrings1 == null) this.PageStrings1 = new List<string>();
                this.PageStrings1.Add(Str);
            }

            foreach (var Str in PageStrings2.Split('|'))
            {
                if (this.PageStrings2 == null) this.PageStrings2 = new List<string>();
                this.PageStrings2.Add(Str);
            }

            this.Items = Items;
            this.Deals = Deals;

            ItemOffers = new HybridDictionary();
            /*
                         foreach (var catalogItem in cataItems.Values.OfType<CatalogItem>().Where(x => x.PageId == id && x.GetFirstBaseItem() != null))
                        {
                            Items.Add(catalogItem.Id, catalogItem);

                            uint flatId = catalogItem.GetFirstBaseItem().FlatId;

                            if (!FlatOffers.ContainsKey(flatId))
                                FlatOffers.Add(catalogItem.GetFirstBaseItem().FlatId, catalogItem.Id);
                        }
                         */
            foreach (var i in flatOffers.Values.OfType<CatalogItem>().Where(x => x.PageID == Id))
            {
                ItemOffers.Add(i.Id, i);
                /*    if (flatOffers[i] == Id)
                        foreach (CatalogItem item in this.Items.Values)
                        {
                            if (item.OfferId == i && !ItemOffers.ContainsKey(i))
                            {
                                ItemOffers.Add(i, item);
                            }
                        }
                */
            }
        }

        public int Id { get; set; }

        public int ParentId { get; set; }

        public bool Enabled { get; set; }

        public string Caption { get; set; }

        public string PageLink { get; set; }

        public int Icon { get; set; }

        public int MinimumRank { get; set; }


        public bool Visible { get; set; }

        public string Template { get; set; }

        public List<string> PageStrings1 { get; }

        public List<string> PageStrings2 { get; }

        public HybridDictionary Items { get; }

        public Dictionary<int, CatalogDeal> Deals { get; private set; }

        public HybridDictionary ItemOffers { get; }
    }
}