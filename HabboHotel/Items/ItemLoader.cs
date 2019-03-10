#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items
{
    public static class ItemLoader
    {
        public static List<Item> GetItemsForRoom(int RoomId, Room Room)
        {
            var I = new List<Item>();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                dbClient.AddParameter("rid", RoomId);
                var Items = dbClient.GetTable();

                if (Items != null)
                    I.AddRange(from DataRow Row in Items.Rows let Data = OblivionServer.GetGame().GetItemManager().GetItem(Convert.ToInt32(Row["base_item"])) where Data != null select new Item(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["room_id"]), Convert.ToInt32(Row["base_item"]), Convert.ToString(Row["extra_data"]), Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]), Convert.ToDouble(Row["z"]), Convert.ToInt32(Row["rot"]), Convert.ToInt32(Row["user_id"]), Convert.ToInt32(Row["group_id"]), Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]), Convert.ToString(Row["wall_pos"]), Room));
            }
            return I;
        }

        /* public static List<Item> GetItemsForUser(int UserId)
        {
            var I = new List<Item>();
            DataTable Items;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = 0 AND `items`.`user_id` = @uid;");
                dbClient.AddParameter("uid", UserId);
                Items = dbClient.getTable();
            }
            if (Items == null) return I;
            foreach (DataRow Row in Items.Rows)
            {
                ItemData Data;
                if (OblivionServer.GetGame().GetItemManager().GetItem(Convert.ToInt32(Row["base_item"]), out Data))
                {
                    I.Add(new Item(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["room_id"]),
                        Convert.ToInt32(Row["base_item"]), Convert.ToString(Row["extra_data"]),
                        Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]), Convert.ToDouble(Row["z"]),
                        Convert.ToInt32(Row["rot"]), Convert.ToInt32(Row["user_id"]),
                        Convert.ToInt32(Row["group_id"]), Convert.ToInt32(Row["limited_number"]),
                        Convert.ToInt32(Row["limited_stack"]), Convert.ToString(Row["wall_pos"])));
                }
            }

            return I;
        }*/
    }
}