#region

using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.LandingView.Items;
using Oblivion.HabboHotel.LandingView.Promotions;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.LandingView
{
    public class LandingViewManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.HabboHotel.LandingView.LandingViewManager");
        private static Dictionary<int, HallOfFameUser> _hallOfFameUsers;

        private readonly Dictionary<int, Promotion> _promotionItems;

        public LandingViewManager()
        {
            _promotionItems = new Dictionary<int, Promotion>();
            _hallOfFameUsers = new Dictionary<int, HallOfFameUser>();

            LoadHallOfFame();

            LoadPromotions();
        }

        public static void LoadHallOfFame()
        {
            if (_hallOfFameUsers.Count > 0)
                _hallOfFameUsers.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,epoints FROM `users` WHERE rank < 3 ORDER BY `epoints` DESC LIMIT 5");
                var GetData = dbClient.GetTable();

                if (GetData != null)
                    foreach (DataRow Row in GetData.Rows)
                        _hallOfFameUsers.Add(Convert.ToInt32(Row["id"]),
                            new HallOfFameUser((int) Row["id"], (int) Row["epoints"]));
            }
            Log.Info("Hall da Fama -> CARREGADO");
        }

        public bool GenerateCalendarItem(Habbo Habbo, string eventName, int eventDate, out Item newItem)
        {
            newItem = null;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `item_id` FROM `campaign_calendar_gifts` WHERE `event_name` = @eventName AND `base_id` = @dayId LIMIT 1");
                dbClient.AddParameter("eventName", eventName);
                dbClient.AddParameter("dayId", eventDate);

                var row = dbClient.GetRow();
                ItemData itemData = OblivionServer.GetGame().GetItemManager().GetItembyName((string)row?["item_id"]);

                if (row?["item_id"] != null && itemData != null)
                {
                    newItem = ItemFactory.CreateSingleItemNullable(itemData, Habbo, "", "");
                    return newItem != null;
                }

                return false;
            }
        }

        public void LoadPromotions()
        {
            _promotionItems.Clear();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_landing` ORDER BY `id` DESC");
                var GetData = dbClient.GetTable();

                if (GetData != null)
                    foreach (DataRow Row in GetData.Rows)
                        _promotionItems.Add(Convert.ToInt32(Row[0]),
                            new Promotion((int) Row[0], Row[1].ToString(), Row[2].ToString(), Row[3].ToString(),
                                Convert.ToInt32(Row[4]), Row[5].ToString(), Row[6].ToString()));
            }


            Log.Info("Vista do Hotel -> LOADED");
        }

        public ICollection<Promotion> GetPromotionItems() => _promotionItems.Values;
        public ICollection<HallOfFameUser> GetHallOfFameUsers() => _hallOfFameUsers.Values;
    }
}