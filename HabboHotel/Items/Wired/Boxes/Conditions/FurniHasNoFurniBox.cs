#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class FurniHasNoFurniBox : IWiredItem
    {
        public FurniHasNoFurniBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.ConditionFurniHasNoFurni;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Option = Packet.PopInt();
            var Unknown2 = Packet.PopString();

            BoolData = Option == 1;

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

        public bool Execute(params object[] Params) => BoolData ? AllFurniHaveNotFurniOn() : SomeFurniHaveNotFurniOn();

        public bool AllFurniHaveNotFurniOn()
        {
            foreach (var Item in SetItems.Values.ToList())
            {
                if (Item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                    continue;

                var NoFurni = false;
                var Items = Instance.GetGameMap().GetAllRoomItemForSquare(Item.GetX, Item.GetY);
                if (!(Items.Count(x => x.GetZ >= Item.GetZ) > 1))
                    NoFurni = true;

                if (!NoFurni)
                    return false;
            }

            return true;
        }

        public bool SomeFurniHaveNotFurniOn() => (from Item in SetItems.Values.ToList()
            where Item != null && Instance.GetRoomItemHandler().GetFloor.Contains(Item)
            select (from I in ItemsData.Split(';')
                where !string.IsNullOrEmpty(I)
                select Instance.GetRoomItemHandler().GetItem(Convert.ToInt32(I))
                into II
                where II != null
                select Instance.GetGameMap().GetAllRoomItemForSquare(II.GetX, II.GetY)).Any(
                Items => !(Items.Count(x => x.GetZ >= Item.GetZ) > 1))).All(NoFurni => NoFurni);
    }
}