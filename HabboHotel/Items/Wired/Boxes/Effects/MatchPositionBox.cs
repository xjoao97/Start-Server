﻿#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Core;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MatchPositionBox : IWiredItem, IWiredCycle
    {
        private double _delay;

        private bool Requested;

        public MatchPositionBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            Requested = false;
        }

        public bool MatchPosition { get; set; }

        public bool MatchRotation { get; set; }

        public bool MatchState { get; set; }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                TickCount = value + 1;
            }
        }

        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (!Requested || string.IsNullOrEmpty(StringData) || StringData == "0;0;0" || SetItems.Count == 0)
                return false;

            foreach (var Item in SetItems.Values.ToList())
            {
                if (Instance.GetRoomItemHandler().GetFloor == null &&
                    !Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                    continue;

                foreach (var I in ItemsData.Split(';'))
                {
                    if (string.IsNullOrEmpty(I))
                        continue;

                    var itemId = Convert.ToInt32(I.Split(':')[0]);
                    var II = Instance.GetRoomItemHandler().GetItem(Convert.ToInt32(itemId));
                    if (II == null)
                        continue;

                    var partsString = I.Split(':');
                    try
                    {
                        if (string.IsNullOrEmpty(partsString[0]) || string.IsNullOrEmpty(partsString[1]))
                            continue;
                    }
                    catch
                    {
                        continue;
                    }

                    var part = partsString[1].Split(',');

                    try
                    {
                        if (int.Parse(StringData.Split(';')[0]) == 1) //State
                            SetState(II, part.Length >= 4 ? part[4] : "1");
                    }
                    catch (Exception e)
                    {
                        Logging.LogWiredException(e.ToString());
                    }

                    try
                    {
                        if (int.Parse(StringData.Split(';')[1]) == 1) //Direction
                            SetRotation(II, Convert.ToInt32(part[3]));
                    }
                    catch (Exception e)
                    {
                        Logging.LogWiredException(e.ToString());
                    }

                    try
                    {
                        if (int.Parse(StringData.Split(';')[2]) == 1) //Position
                            SetPosition(II, Convert.ToInt32(part[0]), Convert.ToInt32(part[1]),
                                Convert.ToDouble(part[2]), Convert.ToInt32(part[3]));
                    }
                    catch (Exception e)
                    {
                        Logging.LogWiredException(e.ToString());
                    }
                }
            }
            Requested = false;
            return true;
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectMatchPosition;

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

            var Delay = Packet.PopInt();
            this.Delay = Delay;
        }

        public bool Execute(params object[] Params)
        {
            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }
            return true;
        }

        private static void SetState(Item Item, string Extradata)
        {
            if (Item.ExtraData == Extradata)
                return;

            if (Item.GetBaseItem().InteractionType == InteractionType.Dice)
                return;

            Item.ExtraData = Extradata;
            Item.UpdateState(false, true);

            if (Item.GetBaseItem().InteractionType == InteractionType.Gate)
            {
                Item.RegenerateBlock(Extradata.ToString(), Item.GetRoom().GetGameMap());
                Item.GetRoom().GetGameMap().updateMapForItem(Item);
            }
        }

        private static void SetRotation(Item Item, int Rotation)
        {
            if (Item.Rotation == Rotation)
                return;

            Item.Rotation = Rotation;
            Item.UpdateState(false, true);
        }

        private void SetPosition(Item Item, int CoordX, int CoordY, double CoordZ, int Rotation)
        {
            Instance.SendMessage(new SlideObjectBundleComposer(Item.GetX, Item.GetY, Item.GetZ, CoordX, CoordY, CoordZ,
                0, 0, Item.Id));

            Instance.GetRoomItemHandler().SetFloorItem(Item, CoordX, CoordY, CoordZ);
            //Instance.GetGameMap().GenerateMaps();
        }
    }
}