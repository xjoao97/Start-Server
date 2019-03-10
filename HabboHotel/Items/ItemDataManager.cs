#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using log4net;
using Oblivion.Core;

#endregion

namespace Oblivion.HabboHotel.Items
{
    public class ItemDataManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Items.ItemDataManager");

        public HybridDictionary Items;

        public ItemDataManager()
        {
            Items = new HybridDictionary();
        }

        public void Init()
        {
            //todo: try to improve
            Items.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `furniture`");
                var ItemData = dbClient.GetTable();
                if (ItemData == null)
                    return;

                foreach (DataRow Row in ItemData.Rows)
                    try
                    {
                        var id = Convert.ToInt32(Row["id"]);
                        var spriteID = Convert.ToInt32(Row["sprite_id"]);
                        var itemName = Convert.ToString(Row["item_name"]);
                        var PublicName = Convert.ToString(Row["public_name"]);
                        var type = Row["type"].ToString();
                        var width = Convert.ToInt32(Row["width"]);
                        var length = Convert.ToInt32(Row["length"]);
                        var height = Convert.ToDouble(Row["stack_height"]);
                        var allowStack = OblivionServer.EnumToBool(Row["can_stack"].ToString());
                        var allowWalk = OblivionServer.EnumToBool(Row["is_walkable"].ToString());
                        var allowSit = OblivionServer.EnumToBool(Row["can_sit"].ToString());
                        var allowRecycle = OblivionServer.EnumToBool(Row["allow_recycle"].ToString());
                        var allowTrade = OblivionServer.EnumToBool(Row["allow_trade"].ToString());
                        var allowMarketplace = Convert.ToInt32(Row["allow_marketplace_sell"]) == 1;
                        var allowGift = Convert.ToInt32(Row["allow_gift"]) == 1;
                        var allowInventoryStack = OblivionServer.EnumToBool(Row["allow_inventory_stack"].ToString());
                        var interactionType =
                            InteractionTypes.GetTypeFromString(Convert.ToString(Row["interaction_type"]));
                        var cycleCount = Convert.ToInt32(Row["interaction_modes_count"]);
                        var vendingIDS = Convert.ToString(Row["vending_ids"]);
                        var heightAdjustable = Convert.ToString(Row["height_adjustable"]);
                        var EffectId = Convert.ToInt32(Row["effect_id"]);
                        var WiredId = Convert.ToInt32(Row["wired_id"]);
                        var IsRare = OblivionServer.EnumToBool(Row["is_rare"].ToString());
                        var ClothingId = Convert.ToInt32(Row["clothing_id"]);
                        var ExtraRot = OblivionServer.EnumToBool(Row["extra_rot"].ToString());
                        var BehaviorData = Convert.ToInt32(Row["behavior_data"].ToString());
                        Items.Add(id, new ItemData(id, spriteID, itemName, PublicName, type, width, length, height,
                            allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace,
                            allowGift, allowInventoryStack, interactionType, cycleCount, vendingIDS,
                            heightAdjustable, EffectId, WiredId, IsRare, ClothingId, ExtraRot, BehaviorData));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.ReadKey();
                        Logging.WriteLine("Could not load item #" + Convert.ToInt32(Row[0]) +
                                          ", please verify the data is okay.");
                    }
            }

            log.Info("Item Manager -> LOADED");
        }

//        public bool GetItembyName(string itemName, out ItemData item)
//        {
//            item = Items.Values.FirstOrDefault(x => x.ItemName == itemName);
//            return item != null;
//        }

//        public bool GetItem(int Id, out ItemData Item) => Items.TryGetValue(Id, out Item);

        internal ItemData GetItem(int id)
        {
            if (Items.Contains(id))
                return (ItemData) Items[id];
            return null;
        }

        internal bool TryGetItem(int itemId, out ItemData item)
        {
            if (Items.Contains(itemId))
            {
                item = (ItemData) Items[itemId];
                return true;
            }
            item = null;
            return false;
           /* foreach (DictionaryEntry entry in Items)
            {
                item = (ItemData) entry.Value;

                if (item.Id == itemId)
                    return true;
            }

            item = null;

            return false;*/
        }

        internal ItemData GetItemBySpriteId(int spriteId)
            =>
                (from DictionaryEntry entry in Items select (ItemData) entry.Value).FirstOrDefault(
                    item => item.SpriteId == spriteId);

        internal ItemData GetItembyName(string itemName)
            =>
                (from DictionaryEntry entry in Items select (ItemData) entry.Value).FirstOrDefault(
                    item => item.ItemName == itemName);

/*
        public ItemData GetItemBySpriteId(int spriteId)
            => Items.Values.FirstOrDefault(value => value.SpriteId == spriteId); //used in gifts x)*/
    }
}