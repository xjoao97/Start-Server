﻿#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class FurniDoesntMatchStateAndPositionBox : IWiredItem
    {
        public FurniDoesntMatchStateAndPositionBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.ConditionDontMatchStateAndPosition;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            var Unknown = Packet.PopInt();
            var State = Packet.PopInt();
            var Direction = Packet.PopInt();
            var Placement = Packet.PopInt();
            var Unknown2 = Packet.PopString();

            var FurniCount = Packet.PopInt();
            for (var i = 0; i < FurniCount; i++)
            {
                var SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());
                if (SelectedItem != null)
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
            }

            StringData = State + ";" + Direction + ";" + Placement;
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData) || StringData == "0;0;0" || SetItems.Count == 0)
                return false;

            foreach (var Item in SetItems.Values.ToList())
            {
                if (!Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                    continue;

                foreach (var I in ItemsData.Split(';'))
                {
                    if (string.IsNullOrEmpty(I))
                        continue;

                    var II = Instance.GetRoomItemHandler().GetItem(Convert.ToInt32(I.Split(':')[0]));
                    if (II == null)
                        continue;

                    var partsString = I.Split(':');
                    var part = partsString[1].Split(',');

                    if (int.Parse(StringData.Split(';')[0]) == 1) //State
                        if (II.ExtraData == part[4])
                            return false;

                    if (int.Parse(StringData.Split(';')[1]) == 1) //Direction
                        if (II.Rotation == Convert.ToInt32(part[3]))
                            return false;

                    if (int.Parse(StringData.Split(';')[2]) == 1) //Position
                        if (II.GetX == Convert.ToInt32(part[0]) && II.GetY == Convert.ToInt32(part[1]) &&
                            II.GetZ == Convert.ToDouble(part[2]))
                            return false;
                }
            }
            return true;
        }
    }
}