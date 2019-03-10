#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using log4net;
using Oblivion.HabboHotel.Catalog.Clothing;
using Oblivion.HabboHotel.Catalog.Marketplace;
using Oblivion.HabboHotel.Catalog.Pets;
using Oblivion.HabboHotel.Catalog.Vouchers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Catalog
{
    public class CatalogManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Catalog.CatalogManager");
        private readonly Dictionary<int, CatalogBot> _botPresets;
        private readonly Dictionary<int, CatalogPage> _CatalogPages;
        private readonly ClothingManager _clothingManager;
        private readonly Dictionary<int, Dictionary<int, CatalogDeal>> _deals;
        private readonly HybridDictionary _items;

        private readonly MarketplaceManager _marketplace;
        private readonly PetRaceManager _petRaceManager;
        private readonly VoucherManager _voucherManager;
        private HybridDictionary _itemOffers;
        private readonly List<DataRow> _index;

        public CatalogManager()
        {
            _marketplace = new MarketplaceManager();
            _petRaceManager = new PetRaceManager();
            _voucherManager = new VoucherManager();
            _clothingManager = new ClothingManager();

            _itemOffers = new HybridDictionary();
            _CatalogPages = new Dictionary<int, CatalogPage>();
            _botPresets = new Dictionary<int, CatalogBot>();
            _items = new HybridDictionary();
            _deals = new Dictionary<int, Dictionary<int, CatalogDeal>>();
            _index = new List<DataRow>();
        }

        public HybridDictionary ItemOffers => _itemOffers;
        public List<DataRow> IndexText => _index;

        public void Init(ItemDataManager ItemDataManager)
        {
            _CatalogPages.Clear();
            _botPresets.Clear();
            _items.Clear();
            _deals.Clear();
            _index.Clear();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                

                dbClient.SetQuery(
                    "SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`, `cost_gotw`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items`");
                var CatalogueItems = dbClient.GetTable();

                if (CatalogueItems != null)
                    foreach (DataRow Row in CatalogueItems.Rows)
                    {
                        if (Convert.ToInt32(Row["amount"]) <= 0)
                            continue;

                        var ItemId = Convert.ToInt32(Row["id"]);
                        var PageId = Convert.ToInt32(Row["page_id"]);
                        var BaseId = Convert.ToInt32(Row["item_id"]);
                        var OfferId = Convert.ToInt32(Row["offer_id"]);

                        if (!ItemDataManager.TryGetItem(BaseId, out ItemData Data))
                        {
                            log.Error("Couldn't load Catalog Item " + ItemId + ", no furniture record found.");
                            continue;
                        }

                        if (!_items.Contains(PageId))
                            _items[PageId] = new HybridDictionary();

                        if (OfferId != -1 && !_itemOffers.Contains(OfferId))
                            _itemOffers.Add(OfferId, PageId);

                        var it = (HybridDictionary) _items[PageId];
                         it.Add(Convert.ToInt32(Row["id"]),
                            new CatalogItem(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["item_id"]),
                                Data, Convert.ToString(Row["catalog_name"]), Convert.ToInt32(Row["page_id"]),
                                Convert.ToInt32(Row["cost_credits"]), Convert.ToInt32(Row["cost_pixels"]),
                                Convert.ToInt32(Row["cost_diamonds"]),
                                Convert.ToInt32(Row["cost_gotw"]),
                                Convert.ToInt32(Row["amount"]), Convert.ToInt32(Row["limited_sells"]),
                                Convert.ToInt32(Row["limited_stack"]),
                                OblivionServer.EnumToBool(Row["offer_active"].ToString()),
                                Convert.ToString(Row["extradata"]), Convert.ToString(Row["badge"]),
                                Convert.ToInt32(Row["offer_id"])));
                    }

                dbClient.SetQuery("SELECT id,page_id,items,name,cost_credits,cost_pixels FROM `catalog_deals`");
                var GetDeals = dbClient.GetTable();

                if (GetDeals != null)
                    foreach (DataRow Row in GetDeals.Rows)
                    {
                        var Id = Convert.ToInt32(Row["id"]);
                        var PageId = Convert.ToInt32(Row["page_id"]);
                        var Items = Convert.ToString(Row["items"]);
                        var Name = Convert.ToString(Row["name"]);
                        var Credits = Convert.ToInt32(Row["cost_credits"]);
                        var Pixels = Convert.ToInt32(Row["cost_pixels"]);

                        if (!_deals.ContainsKey(PageId))
                            _deals[PageId] = new Dictionary<int, CatalogDeal>();

                        var Deal = new CatalogDeal(Id, PageId, Items, Name, Credits, Pixels, ItemDataManager);
                        _deals[PageId].Add(Deal.Id, Deal);
                    }


                dbClient.SetQuery(
                    "SELECT `id`,`parent_id`,`caption`,`page_link`,`visible`,`enabled`,`min_rank`,`min_vip`,`icon_image`,`page_layout`,`page_strings_1`,`page_strings_2` FROM `catalog_pages` ORDER BY `order_num`");
                var CatalogOblivionServer = dbClient.GetTable();

                if (CatalogOblivionServer != null)
                    foreach (DataRow Row in CatalogOblivionServer.Rows)
                        _CatalogPages.Add(Convert.ToInt32(Row["id"]),
                            new CatalogPage(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["parent_id"]),
                                Row["enabled"].ToString(), Convert.ToString(Row["caption"]),
                                Convert.ToString(Row["page_link"]), Convert.ToInt32(Row["icon_image"]),
                                Convert.ToInt32(Row["min_rank"]), Convert.ToInt32(Row["min_vip"]),
                                Row["visible"].ToString(), Convert.ToString(Row["page_layout"]),
                                Convert.ToString(Row["page_strings_1"]), Convert.ToString(Row["page_strings_2"]),
                                (HybridDictionary) (_items.Contains(Convert.ToInt32(Row["id"]))
                                    ? _items[Convert.ToInt32(Row["id"])]
                                    : new HybridDictionary()),
                                _deals.ContainsKey(Convert.ToInt32(Row["id"]))
                                    ? _deals[Convert.ToInt32(Row["id"])]
                                    : new Dictionary<int, CatalogDeal>(), ref _itemOffers));

                dbClient.SetQuery(
                    "SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
                var bots = dbClient.GetTable();

                if (bots != null)
                    foreach (DataRow Row in bots.Rows)
                        _botPresets.Add(Convert.ToInt32(Row[0]),
                            new CatalogBot(Convert.ToInt32(Row[0]), Convert.ToString(Row[1]),
                                Convert.ToString(Row[2]),
                                Convert.ToString(Row[3]), Convert.ToString(Row[4]), Convert.ToString(Row[5])));

                _petRaceManager.Init();
                _clothingManager.Init();
            }

            log.Info("Catálogo -> CARREGADO");
        }

        public bool TryGetBot(int ItemId, out CatalogBot Bot) => _botPresets.TryGetValue(ItemId, out Bot);

        public bool TryGetPage(int pageId, out CatalogPage page) => _CatalogPages.TryGetValue(pageId, out page);

        public ICollection<CatalogPage> GetPages() => _CatalogPages.Values;

        public MarketplaceManager GetMarketplace() => _marketplace;

        public PetRaceManager GetPetRaceManager() => _petRaceManager;

        public VoucherManager GetVoucherManager() => _voucherManager;

        public ClothingManager GetClothingManager() => _clothingManager;
    }
}