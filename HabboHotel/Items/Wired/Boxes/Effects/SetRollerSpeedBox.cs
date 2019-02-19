﻿#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class SetRollerSpeedBox : IWiredItem
    {
        public SetRollerSpeedBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();

            if (SetItems.Count > 0)
                SetItems.Clear();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectSetRollerSpeed;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            var Unknown = Packet.PopInt();
            var Message = Packet.PopString();

            StringData = Message;

            int Speed;
            if (!int.TryParse(StringData, out Speed))
                StringData = "";
        }

        public bool Execute(params object[] Params)
        {
            int Speed;
            if (int.TryParse(StringData, out Speed))
                Instance.GetRoomItemHandler().SetSpeed(Speed);
            return true;
        }
    }
}