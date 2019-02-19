#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class ToggleFurniBox : IWiredItem, IWiredCycle
    {
        private double _delay;

        private double _next;
        private bool Requested;

        public ToggleFurniBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public double TickCount { get; set; }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                TickCount = value;
            }
        }

        public bool OnCycle()
        {
            if (SetItems.Count == 0 || !Requested)
                return false;

            var Now = OblivionServer.GetUnixTimestamp();
            if (_next <= Now)
            {
                foreach (var Item in SetItems.Values.ToList().Where(Item => Item != null))
                {
                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                    {
                        Item n;
                        SetItems.TryRemove(Item.Id, out n);
                        continue;
                    }

                    Item.Interactor.OnWiredTrigger(Item);
                }

                Requested = false;

                _next = 0;
                TickCount = Delay;
            }
            return true;
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectToggleFurniState;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            SetItems.Clear();
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();

            var FurniCount = Packet.PopInt();
            for (var i = 0; i < FurniCount; i++)
            {
                var SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());
                if (SelectedItem != null)
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
            }

            var Delay = Packet.PopInt();
            this.Delay = Delay;
        }

        public bool Execute(params object[] Params)
        {
            if (_next == 0 || _next < OblivionServer.GetUnixTimestamp())
                 _next = OblivionServer.GetUnixTimestamp() + Delay;


            Requested = true;
            TickCount = Delay;
            return true;
        }
    }
}