#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Navigator
{
    public sealed class NavigatorManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.HabboHotel.Navigator.NavigatorManager");

        private readonly Dictionary<int, FeaturedRoom> _featuredRooms;
        private readonly Dictionary<int, SearchResultList> _searchResultLists;

        private readonly Dictionary<int, TopLevelItem> _topLevelItems;

        public NavigatorManager()
        {
            _topLevelItems = new Dictionary<int, TopLevelItem>();
            _searchResultLists = new Dictionary<int, SearchResultList>();

            //Does this need to be dynamic?
            _topLevelItems.Add(1, new TopLevelItem(1, "official_view", "", ""));
            _topLevelItems.Add(2, new TopLevelItem(2, "hotel_view", "", ""));
//            _topLevelItems.Add(3, new TopLevelItem(3, "roomads_view", "", ""));
            _topLevelItems.Add(4, new TopLevelItem(4, "myworld_view", "", ""));

            _featuredRooms = new Dictionary<int, FeaturedRoom>();

            Init();
        }

        public void Init()
        {
            if (_searchResultLists.Count > 0)
                _searchResultLists.Clear();

            if (_featuredRooms.Count > 0)
                _featuredRooms.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `navigator_categories` ORDER BY `id` ASC");
                var Table = dbClient.getTable();

                if (Table != null)
                    foreach (var Row in from DataRow Row in Table.Rows
                        where Convert.ToInt32(Row["enabled"]) == 1
                        where !_searchResultLists.ContainsKey(Convert.ToInt32(Row["id"]))
                        select Row)
                        _searchResultLists.Add(Convert.ToInt32(Row["id"]),
                            new SearchResultList(Convert.ToInt32(Row["id"]), Convert.ToString(Row["category"]),
                                Convert.ToString(Row["category_identifier"]),
                                Convert.ToString(Row["public_name"]), true, -1,
                                Convert.ToInt32(Row["required_rank"]),
                                NavigatorViewModeUtility.GetViewModeByString(Convert.ToString(Row["view_mode"])),
                                Convert.ToString(Row["category_type"]),
                                Convert.ToString(Row["search_allowance"]), Convert.ToInt32(Row["order_id"])));

                dbClient.SetQuery(
                    "SELECT `room_id`,`caption`,`description`,`image_url`,`enabled` FROM `navigator_publics` ORDER BY `order_num` ASC");
                var GetPublics = dbClient.getTable();

                if (GetPublics != null)
                    foreach (var Row in from DataRow Row in GetPublics.Rows
                        where Convert.ToInt32(Row["enabled"]) == 1
                        where !_featuredRooms.ContainsKey(Convert.ToInt32(Row["room_id"]))
                        select Row)
                        _featuredRooms.Add(Convert.ToInt32(Row["room_id"]),
                            new FeaturedRoom(Convert.ToInt32(Row["room_id"]), Convert.ToString(Row["caption"]),
                                Convert.ToString(Row["description"]), Convert.ToString(Row["image_url"])));
            }

            Log.Info("Navigator -> LOADED");
        }

        public List<SearchResultList> GetCategorysForSearch(string Category)
        {
            var Categorys =
                from Cat in _searchResultLists
                where Cat.Value.Category == Category
                orderby Cat.Value.OrderId ascending
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<SearchResultList> GetResultByIdentifier(string Category)
        {
            var Categorys =
                from Cat in _searchResultLists
                where Cat.Value.CategoryIdentifier == Category
                orderby Cat.Value.OrderId ascending
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<SearchResultList> GetFlatCategories()
        {
            var Categorys =
                from Cat in _searchResultLists
                where Cat.Value.CategoryType == NavigatorCategoryType.CATEGORY
                orderby Cat.Value.OrderId ascending
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<SearchResultList> GetEventCategories()
        {
            var Categorys =
                from Cat in _searchResultLists
                where Cat.Value.CategoryType == NavigatorCategoryType.PROMOTION_CATEGORY
                orderby Cat.Value.OrderId ascending
                select Cat.Value;
            return Categorys.ToList();
        }

        public ICollection<TopLevelItem> GetTopLevelItems() => _topLevelItems.Values;

        public ICollection<SearchResultList> GetSearchResultLists() => _searchResultLists.Values;

        public bool TryGetTopLevelItem(int Id, out TopLevelItem TopLevelItem)
            => _topLevelItems.TryGetValue(Id, out TopLevelItem);

        public bool TryGetSearchResultList(int Id, out SearchResultList SearchResultList)
            => _searchResultLists.TryGetValue(Id, out SearchResultList);

        public bool TryGetFeaturedRoom(int RoomId, out FeaturedRoom PublicRoom)
            => _featuredRooms.TryGetValue(RoomId, out PublicRoom);

        public ICollection<FeaturedRoom> GetFeaturedRooms() => _featuredRooms.Values;
    }
}