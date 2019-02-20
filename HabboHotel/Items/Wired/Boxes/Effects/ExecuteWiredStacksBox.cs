#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class ExecuteWiredStacksBox : IWiredItem
    {
        public ExecuteWiredStacksBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectExecuteWiredStacks;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();

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
            if (Params.Length != 1)
                return false;

            var Player = (Habbo)Params[0];
            if (Player == null)
                return false;

            foreach (var Item in SetItems.Values.ToList().Where(Item => Item != null && Instance.GetRoomItemHandler().GetFloor.Contains(Item) && Item.IsWired))
            {
                IWiredItem WiredItem = Instance.GetWired().GetWired(Item.Id);
                if (WiredItem == null)
                    continue;

                if (WiredItem.Type != WiredBoxType.EffectExecuteWiredStacks && WiredItem.Type != WiredBoxType.TriggerRepeat)
                {
                    WiredItem.Execute(Player);
                    Instance?.GetWired().OnEvent(WiredItem.Item);
                }
            }

            return true;
        }
    }
}