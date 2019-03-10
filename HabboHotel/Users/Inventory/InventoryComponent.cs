#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Users.Inventory.Bots;
using Oblivion.HabboHotel.Users.Inventory.Pets;

#endregion

namespace Oblivion.HabboHotel.Users.Inventory
{
    public class InventoryComponent
    {
        private readonly int _userId;
        public ConcurrentDictionary<int, Bot> _botItems;
        private GameClient _client;
        public ConcurrentDictionary<int, Item> _floorItems;
        public ConcurrentDictionary<int, Pet> _petsItems;
        public ConcurrentDictionary<int, Item> _wallItems;
//        private readonly object _lock = new object();
//todo: hybrid dictionary
        public InventoryComponent(int UserId, GameClient Client)
        {
            _client = Client;
            _userId = UserId;

            _floorItems = new ConcurrentDictionary<int, Item>();
            _wallItems = new ConcurrentDictionary<int, Item>();
            _petsItems = new ConcurrentDictionary<int, Pet>();
            _botItems = new ConcurrentDictionary<int, Bot>();
            Init();
        }

        public IEnumerable<Item> GetItems => _floorItems.Values.Concat(_wallItems.Values);

        public void Init()
        {
            _floorItems.Clear();
            _wallItems.Clear();
            _petsItems.Clear();
            _botItems.Clear();

            DataTable Items;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = 0 AND `items`.`user_id` = @uid;");
                dbClient.AddParameter("uid", _userId);
                Items = dbClient.getTable();
            }
            if (Items == null) return;
            foreach (var item in from DataRow Row in Items.Rows
                let Data = OblivionServer.GetGame()
                    .GetItemManager()
                    .GetItem(Convert.ToInt32(Row["base_item"]))
                where Data != null
                select new Item(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["room_id"]),
                    Convert.ToInt32(Row["base_item"]), Convert.ToString(Row["extra_data"]),
                    Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]), Convert.ToDouble(Row["z"]),
                    Convert.ToInt32(Row["rot"]), Convert.ToInt32(Row["user_id"]),
                    Convert.ToInt32(Row["group_id"]), Convert.ToInt32(Row["limited_number"]),
                    Convert.ToInt32(Row["limited_stack"]), Convert.ToString(Row["wall_pos"])))
            {
                if (item.IsFloorItem)
                    _floorItems.TryAdd(item.Id, item);
                else
                    _wallItems.TryAdd(item.Id, item);
            }
            var Pets = PetLoader.GetPetsForUser(Convert.ToInt32(_userId));
            foreach (var Pet in Pets.Where(Pet => !_petsItems.TryAdd(Pet.PetId, Pet)))
                Console.WriteLine("Error whilst loading pet x1: " + Pet.PetId);

            var Bots = BotLoader.GetBotsForUser(Convert.ToInt32(_userId));
            foreach (var Bot in Bots)
            {
                _botItems.TryAdd(Bot.Id, Bot);
            }
        }


        public void ClearItems() => Task.Factory.StartNew(() =>
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("DELETE FROM items WHERE room_id='0' AND user_id = " + _userId); //Do join 
            }

            _floorItems.Clear();
            _wallItems.Clear();

            UpdateItems(false);
        });

        public void SetIdleState()
        {
            _botItems?.Clear();

            _petsItems?.Clear();

            _floorItems?.Clear();

            _wallItems?.Clear();

            _botItems = null;
            _petsItems = null;
            _floorItems = null;
            _wallItems = null;

            _client = null;
        }


        public void UpdateItems(bool FromDatabase)
        {
            if (FromDatabase)
                Init();
            _client?.SendMessage(new FurniListUpdateComposer());
        }

        public Item GetItem(int Id)
        {
            if (_floorItems.ContainsKey(Id))
                return _floorItems[Id];
            return _wallItems.ContainsKey(Id) ? _wallItems[Id] : null;
        }


        internal void AddItem(Item item)
            => AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, 0, 0);

        public Item AddNewItem(int Id, int BaseItem, string ExtraData, int Group, bool ToInsert, bool FromRoom,
            int LimitedNumber, int LimitedStack)
        {
            Task.Factory.StartNew(() =>
            {
                if (ToInsert)
                    if (FromRoom)
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunFastQuery("UPDATE `items` SET `room_id` = '0', `user_id` = '" + _userId +
                                              "' WHERE `id` = '" + Id + "' LIMIT 1");
                        }
                    else
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery(
                                "INSERT INTO `items` (`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ('" +
                                BaseItem + "', '" + _userId + "', '" + LimitedNumber + "', '" +
                                LimitedStack + "')");
                            if (Id == 0)
                                Id = (int) dbClient.InsertQuery();

                            SendNewItems(Id);

                            if (Group > 0)
                                dbClient.RunFastQuery("INSERT INTO `items_groups` VALUES (" + Id + ", " + Group + ")");

                            if (!string.IsNullOrEmpty(ExtraData))
                            {
                                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @extradata WHERE `id` = '" + Id +
                                                  "' LIMIT 1");
                                dbClient.AddParameter("extradata", ExtraData);
                                dbClient.RunQuery();
                            }
                        }
                UpdateItems(true);
            });
            if (Id == 0)
                return null;

            var ItemToAdd = new Item(Id, 0, BaseItem, ExtraData, 0, 0, 0, 0, _userId, Group, LimitedNumber,
                LimitedStack,
                string.Empty);

            if (UserHoldsItem(Id))
                RemoveItem(Id);

            if (ItemToAdd.IsWallItem)
                _wallItems.TryAdd(ItemToAdd.Id, ItemToAdd);
            else if (ItemToAdd.IsFloorItem)
                _floorItems.TryAdd(ItemToAdd.Id, ItemToAdd);

            UpdateItems(true);
            return ItemToAdd;
        }

        private bool UserHoldsItem(int itemID) => _floorItems.ContainsKey(itemID) || _wallItems.ContainsKey(itemID);

        public void RemoveItem(int Id)
        {
            if (GetClient() == null)
                return;

            if (GetClient().GetHabbo() == null || GetClient().GetHabbo().GetInventoryComponent() == null)
                GetClient().Disconnect();

            if (_floorItems.ContainsKey(Id))
            {
                Item ToRemove;
                _floorItems.TryRemove(Id, out ToRemove);
            }

            if (_wallItems.ContainsKey(Id))
            {
                Item ToRemove;
                _wallItems.TryRemove(Id, out ToRemove);
            }

            GetClient().SendMessage(new FurniListRemoveComposer(Id));
        }

        private GameClient GetClient() => OblivionServer.GetGame().GetClientManager().GetClientByUserID(_userId);

        public void SendNewItems(int Id) => _client.SendMessage(new FurniListNotificationComposer(Id, 1));

        public bool TryAddItem(Item item)
        {
            if (item.Data.Type.ToString().ToLower() == "s") // ItemType.FLOOR)
                return _floorItems.TryAdd(item.Id, item);
            if (item.Data.Type.ToString().ToLower() == "i") //ItemType.WALL)
                return _wallItems.TryAdd(item.Id, item);
            throw new InvalidOperationException("Item did not match neither floor or wall item");
        }

        public ICollection<Item> GetFloorItems() => _floorItems.Values;

        public ICollection<Item> GetWallItems() => _wallItems.Values;

        #region Pet Handling

        public ICollection<Pet> GetPets() => _petsItems.Values;

        public bool TryAddPet(Pet Pet)
        {
            //TODO: Sort this mess.
            Pet.RoomId = 0;
            Pet.PlacedInRoom = false;

            return _petsItems.TryAdd(Pet.PetId, Pet);
        }

        public bool TryRemovePet(int PetId, out Pet PetItem)
        {
            if (_petsItems.ContainsKey(PetId))
                return _petsItems.TryRemove(PetId, out PetItem);
            PetItem = null;
            return false;
        }

        public bool TryGetPet(int PetId, out Pet Pet)
        {
            if (_petsItems.ContainsKey(PetId))
                return _petsItems.TryGetValue(PetId, out Pet);
            Pet = null;
            return false;
        }

        #endregion

        #region Bot Handling

        public ICollection<Bot> GetBots() => _botItems.Values;

        public bool TryAddBot(Bot Bot) => _botItems.TryAdd(Bot.Id, Bot);

        public bool TryRemoveBot(int BotId, out Bot Bot)
        {
            if (_botItems.ContainsKey(BotId))
                return _botItems.TryRemove(BotId, out Bot);
            Bot = null;
            return false;
        }

        public bool TryGetBot(int BotId, out Bot Bot)
        {
            if (_botItems.ContainsKey(BotId))
                return _botItems.TryGetValue(BotId, out Bot);
            Bot = null;
            return false;
        }

        #endregion
    }
}