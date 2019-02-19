﻿#region

using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class FurniHasNoUsersBox : IWiredItem
    {
        public FurniHasNoUsersBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.ConditionFurniHasNoUsers;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            var FurniCount = Packet.PopInt();
            for (var i = 0; i < FurniCount; i++)
            {
                var SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());
                if (SelectedItem != null)
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
            }
        }

        public bool Execute(params object[] Params)
        {
            foreach (var Item in SetItems.Values.ToList())
            {
                if (Item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                    continue;

                var HasUsers = false;
                foreach (var Tile in Item.GetAffectedTiles.Values)
                    if (Instance.GetGameMap().SquareHasUsers(Tile.X, Tile.Y))
                        HasUsers = true;

                if (Instance.GetGameMap().SquareHasUsers(Item.GetX, Item.GetY))
                    HasUsers = true;

                if (HasUsers)
                    return false;
            }
            return true;
        }
    }
}