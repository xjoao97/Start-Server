﻿#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items
{
    public class ItemFactory
    {
        public static Item CreateSingleItemNullable(ItemData Data, Habbo Habbo, string ExtraData, string DisplayFlags,
            int GroupId = 0, int LimitedNumber = 0, int LimitedStack = 0)
        {
            if (Data == null) throw new InvalidOperationException("Data cannot be null.");

            var Item = new Item(0, 0, Data.Id, ExtraData, 0, 0, 0, 0, Habbo.Id, GroupId, LimitedNumber, LimitedStack, "");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                dbClient.AddParameter("did", Data.Id);
                dbClient.AddParameter("uid", Habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wall_pos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("extra_data", ExtraData);
                dbClient.AddParameter("limited_number", LimitedNumber);
                dbClient.AddParameter("limited_stack", LimitedStack);
                Item.Id = Convert.ToInt32(dbClient.InsertQuery());

                if (GroupId > 0)
                {
                    dbClient.SetQuery("INSERT INTO `items_groups` (`id`, `group_id`) VALUES (@id, @gid)");
                    dbClient.AddParameter("id", Item.Id);
                    dbClient.AddParameter("gid", GroupId);
                    dbClient.RunQuery();
                }
                return Item;
            }
        }

        public static Item CreateSingleItem(ItemData Data, Habbo Habbo, string ExtraData, string DisplayFlags)
        {
            if (Data == null) throw new InvalidOperationException("Data cannot be null.");
//            new Thread(() =>
//            {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                dbClient.AddParameter("did", Data.Id);
                dbClient.AddParameter("uid", Habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wall_pos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("extra_data", ExtraData);
                dbClient.AddParameter("limited_number", 0);
                dbClient.AddParameter("limited_stack", 0);
                dbClient.RunQuery();

                var Item = new Item(Convert.ToInt32(dbClient.InsertQuery()), 0, Data.Id, ExtraData, 0, 0, 0, 0,
                    Habbo.Id,
                    0, 0, 0, "");
                return Item;
            }
            //            }).Start();
        }


        public static Item CreateSingleItem(ItemData Data, Habbo Habbo, string ExtraData, string DisplayFlags,
            int ItemId, int LimitedNumber = 0, int LimitedStack = 0)
        {
            if (Data == null) throw new InvalidOperationException("Data cannot be null.");

            var Item = new Item(ItemId, 0, Data.Id, ExtraData, 0, 0, 0, 0, Habbo.Id, 0, LimitedNumber, LimitedStack, "");
            new Task(() =>
            {
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@id, @did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                    dbClient.AddParameter("id", ItemId);
                    dbClient.AddParameter("did", Data.Id);
                    dbClient.AddParameter("uid", Habbo.Id);
                    dbClient.AddParameter("rid", 0);
                    dbClient.AddParameter("x", 0);
                    dbClient.AddParameter("y", 0);
                    dbClient.AddParameter("z", 0);
                    dbClient.AddParameter("wall_pos", "");
                    dbClient.AddParameter("rot", 0);
                    dbClient.AddParameter("extra_data", ExtraData);
                    dbClient.AddParameter("limited_number", LimitedNumber);
                    dbClient.AddParameter("limited_stack", LimitedStack);
                    dbClient.RunQuery();
                }
            }).Start();
            return Item;
        }

        public static Item CreateGiftItem(ItemData Data, Habbo Habbo, string ExtraData, string DisplayFlags, int ItemId,
            int LimitedNumber = 0, int LimitedStack = 0)
        {
            if (Data == null) throw new InvalidOperationException("Data cannot be null.");

            var Item = new Item(ItemId, 0, Data.Id, ExtraData, 0, 0, 0, 0, Habbo.Id, 0, LimitedNumber, LimitedStack, "");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@id, @did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                dbClient.AddParameter("id", ItemId);
                dbClient.AddParameter("did", Data.Id);
                dbClient.AddParameter("uid", Habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wall_pos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("extra_data", ExtraData);
                dbClient.AddParameter("limited_number", LimitedNumber);
                dbClient.AddParameter("limited_stack", LimitedStack);
                dbClient.RunQuery();

                return Item;
            }
        }

        public static List<Item> CreateMultipleItems(ItemData Data, Habbo Habbo, string ExtraData, int Amount,
            int GroupId = 0)
        {
            if (Data == null) throw new InvalidOperationException("Data cannot be null.");
            if (!CanFactoryFurni(Habbo.GetClient(), ExtraData, GroupId, Data))
                return new List<Item>();
            var Items = new List<Item>();
            Task.Factory.StartNew(() =>
            {
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    for (var i = 0; i < Amount; i++)
                    {
                        dbClient.SetQuery(
                            "INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
                        dbClient.AddParameter("did", Data.Id);
                        dbClient.AddParameter("uid", Habbo.Id);
                        dbClient.AddParameter("rid", 0);
                        dbClient.AddParameter("x", 0);
                        dbClient.AddParameter("y", 0);
                        dbClient.AddParameter("z", 0);
                        dbClient.AddParameter("wallpos", "");
                        dbClient.AddParameter("rot", 0);
                        dbClient.AddParameter("flags", ExtraData);

                        var Item = new Item(Convert.ToInt32(dbClient.InsertQuery()), 0, Data.Id, ExtraData, 0, 0, 0, 0,
                            Habbo.Id, GroupId, 0, 0, "");

                        if (GroupId > 0)
                        {
                            dbClient.SetQuery("INSERT INTO `items_groups` (`id`, `group_id`) VALUES (@id, @gid)");
                            dbClient.AddParameter("id", Item.Id);
                            dbClient.AddParameter("gid", GroupId);
                            dbClient.RunQuery();
                        }

                        Items.Add(Item);
                    }
                }
                Habbo.GetInventoryComponent().UpdateItems(true);
            });
            return Items;
        }

        public static bool CanFactoryFurni(GameClient Session, string Extradata, int GroupId, ItemData Data)
        {
            if (Data.InteractionType != InteractionType.GuildForum) return true;
            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return false;

            if (Group.CreatorId == Session.GetHabbo().Id || Group.HasForum) return true;
            Session.SendNotification("Can't create item");
            return false;
        }

        public static List<Item> CreateTeleporterItems(ItemData Data, Habbo Habbo, int GroupId = 0)
        {
            var Items = new List<Item>();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
                dbClient.AddParameter("did", Data.Id);
                dbClient.AddParameter("uid", Habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wallpos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("flags", "");

                var Item1Id = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery(
                    "INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
                dbClient.AddParameter("did", Data.Id);
                dbClient.AddParameter("uid", Habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wallpos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("flags", Item1Id.ToString());

                var Item2Id = Convert.ToInt32(dbClient.InsertQuery());

                var Item1 = new Item(Item1Id, 0, Data.Id, "", 0, 0, 0, 0, Habbo.Id, GroupId, 0, 0, "");
                var Item2 = new Item(Item2Id, 0, Data.Id, "", 0, 0, 0, 0, Habbo.Id, GroupId, 0, 0, "");

                dbClient.SetQuery("INSERT INTO `room_items_tele_links` (`tele_one_id`, `tele_two_id`) VALUES (" +
                                  Item1Id + ", " + Item2Id + "), (" + Item2Id + ", " + Item1Id + ")");
                dbClient.RunQuery();

                Items.Add(Item1);
                Items.Add(Item2);
            }
            return Items;
        }

        public static void CreateMoodlightData(Item Item)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `room_items_moodlight` (`id`, `enabled`, `current_preset`, `preset_one`, `preset_two`, `preset_three`) VALUES (@id, '0', 1, @preset, @preset, @preset);");
                dbClient.AddParameter("id", Item.Id);
                dbClient.AddParameter("preset", "#000000,255,0");
                dbClient.RunQuery();
            }
        }

        public static void CreateTonerData(Item Item)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `room_items_toner` (`id`, `data1`, `data2`, `data3`, `enabled`) VALUES (@id, 0, 0, 0, '0')");
                dbClient.AddParameter("id", Item.Id);
                dbClient.RunQuery();
            }
        }
    }
}