#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class GameItemHandler
    {
        private ConcurrentDictionary<int, Item> _banzaiPyramids;
        private ConcurrentDictionary<int, Item> _banzaiTeleports;
        private Random rnd;
        private Room room;

        public GameItemHandler(Room room)
        {
            this.room = room;
            rnd = new Random();

            _banzaiPyramids = new ConcurrentDictionary<int, Item>();
            _banzaiTeleports = new ConcurrentDictionary<int, Item>();
        }

        public void OnCycle() => CyclePyramids();

        private void CyclePyramids()
        {
            var rnd = new Random();

            foreach (var item in _banzaiPyramids.Values.ToList().Where(item => item != null))
            {
                if (item.interactionCountHelper == 0 && item.ExtraData == "1")
                {
                    room.GetGameMap().RemoveFromMap(item, false);
                    item.interactionCountHelper = 1;
                }

                if (string.IsNullOrEmpty(item.ExtraData))
                    item.ExtraData = "0";

                var randomNumber = rnd.Next(0, 30);
                if (randomNumber == 15)
                    if (item.ExtraData == "0")
                    {
                        item.ExtraData = "1";
                        item.UpdateState();
                        room.GetGameMap().RemoveFromMap(item, false);
                    }
                    else
                    {
                        if (room.GetGameMap().itemCanBePlacedHere(item.GetX, item.GetY))
                        {
                            item.ExtraData = "0";
                            item.UpdateState();
                            room.GetGameMap().AddItemToMap(item);
                        }
                    }
            }
        }

        public void AddPyramid(Item item, int itemID)
        {
            if (_banzaiPyramids.ContainsKey(itemID))
                _banzaiPyramids[itemID] = item;
            else
                _banzaiPyramids.TryAdd(itemID, item);
        }

        public void RemovePyramid(int itemID)
        {
            Item Item;
            _banzaiPyramids.TryRemove(itemID, out Item);
        }

        public void AddTeleport(Item item, int itemID)
        {
            if (_banzaiTeleports.ContainsKey(itemID))
                _banzaiTeleports[itemID] = item;
            else
                _banzaiTeleports.TryAdd(itemID, item);
        }

        public void RemoveTeleport(int itemID)
        {
            Item Item;
            _banzaiTeleports.TryRemove(itemID, out Item);
        }

        public void onTeleportRoomUserEnter(RoomUser User, Item Item)
        {
            var items = _banzaiTeleports.Values.Where(p => p.Id != Item.Id);

            var count = items.Count();

            var countID = rnd.Next(0, count);
            var countAmount = 0;

            if (count == 0)
                return;

            foreach (var item in items.ToList().Where(item => item != null))
            {
                if (countAmount == countID)
                {
                    item.ExtraData = "1";
                    item.UpdateNeeded = true;

                    room.GetGameMap().TeleportToItem(User, item);

                    Item.ExtraData = "1";
                    Item.UpdateNeeded = true;
                    item.UpdateState();
                    Item.UpdateState();
                }

                countAmount++;
            }
        }

        public void Dispose()
        {
            _banzaiTeleports?.Clear();
            _banzaiPyramids?.Clear();
            _banzaiPyramids = null;
            _banzaiTeleports = null;
            room = null;
            rnd = null;
        }
    }
}