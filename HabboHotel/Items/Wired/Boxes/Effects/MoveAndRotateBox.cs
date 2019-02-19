﻿#region

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MoveAndRotateBox : IWiredItem, IWiredCycle
    {
        private double _delay;
        private double _next;
        private bool Requested;

        public MoveAndRotateBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            Requested = false;
        }

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
            if (Instance == null || !Requested || _next < 1)
                return false;

            var Now = OblivionServer.GetUnixTimestamp();
            if (_next <= Now)
            {
                foreach (
                    var Item in
                    SetItems.Values.ToList()
                        .Where(Item => Item != null && Instance.GetRoomItemHandler().GetFloor.Contains(Item)))
                {
                    Item toRemove;

                    if (Instance.GetWired().OtherBoxHasItem(this, Item.Id))
                        SetItems.TryRemove(Item.Id, out toRemove);


                    var Point = HandleMovement(Convert.ToInt32(StringData.Split(';')[0]),
                        new Point(Item.GetX, Item.GetY));
                    var newRot = HandleRotation(Convert.ToInt32(StringData.Split(';')[1]), Item.Rotation);

                    Instance.GetWired().onUserFurniCollision(Instance, Item);

                    if (!Instance.GetGameMap().ItemCanMove(Item, Point))
                        continue;

                    if (Instance.GetGameMap().CanRollItemHere(Point.X, Point.Y) &&
                        !Instance.GetGameMap().SquareHasUsers(Point.X, Point.Y))
                    {
                        var NewZ = Instance.GetGameMap().GetHeightForSquareFromData(Point);
                        var CanBePlaced = true;

                        var Items = Instance.GetGameMap().GetCoordinatedItems(Point);
                        foreach (var IItem in Items.ToList().Where(IItem => IItem != null && IItem.Id != Item.Id))
                        {
                            if (!IItem.GetBaseItem().Walkable)
                            {
                                _next = 0;
                                CanBePlaced = false;
                                break;
                            }

                            if (IItem.TotalHeight > NewZ)
                                NewZ = IItem.TotalHeight;

                            if (CanBePlaced && !IItem.GetBaseItem().Stackable)
                                CanBePlaced = false;
                        }

                        if (newRot != Item.Rotation)
                        {
                            Item.Rotation = newRot;
                            Item.UpdateState(false, true);
                        }

                        if (CanBePlaced && Point != Item.Coordinate)
                        {
                            Instance.SendMessage(new SlideObjectBundleComposer(Item.GetX, Item.GetY, Item.GetZ, Point.X,
                                Point.Y, NewZ, 0, 0, Item.Id));
                            Instance.GetRoomItemHandler().SetFloorItem(Item, Point.X, Point.Y, NewZ);
                        }
                    }
                }

                _next = 0;
                return true;
            }
            return false;
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectMoveAndRotate;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            var Unknown = Packet.PopInt();
            var Movement = Packet.PopInt();
            var Rotation = Packet.PopInt();

            var Unknown1 = Packet.PopString();

            var FurniCount = Packet.PopInt();
            for (var i = 0; i < FurniCount; i++)
            {
                var SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());

                if (SelectedItem != null && !Instance.GetWired().OtherBoxHasItem(this, SelectedItem.Id))
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
            }

            StringData = Movement + ";" + Rotation;
            Delay = Packet.PopInt();
        }

        public bool Execute(params object[] Params)
        {
            if (SetItems.Count == 0)
                return false;

            if (_next < 1 || _next < OblivionServer.GetUnixTimestamp())
                 _next = OblivionServer.GetUnixTimestamp() + Delay;

            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }
            return true;
        }

        private int HandleRotation(int mode, int rotation)
        {
            switch (mode)
            {
                case 1:
                {
                    rotation += 2;
                    if (rotation > 6)
                        rotation = 0;
                    break;
                }

                case 2:
                {
                    rotation -= 2;
                    if (rotation < 0)
                        rotation = 6;
                    break;
                }

                case 3:
                {
                    if (RandomNumber.GenerateRandom(0, 2) == 0)
                    {
                        rotation += 2;
                        if (rotation > 6)
                            rotation = 0;
                    }
                    else
                    {
                        rotation -= 2;
                        if (rotation < 0)
                            rotation = 6;
                    }
                    break;
                }
            }
            return rotation;
        }

        private static Point HandleMovement(int Mode, Point Position)
        {
            var NewPos = new Point();
            switch (Mode)
            {
                case 0:
                {
                    NewPos = Position;
                    break;
                }
                case 1:
                {
                    switch (RandomNumber.GenerateRandom(1, 4))
                    {
                        case 1:
                            NewPos = new Point(Position.X + 1, Position.Y);
                            break;
                        case 2:
                            NewPos = new Point(Position.X - 1, Position.Y);
                            break;
                        case 3:
                            NewPos = new Point(Position.X, Position.Y + 1);
                            break;
                        case 4:
                            NewPos = new Point(Position.X, Position.Y - 1);
                            break;
                    }
                    break;
                }
                case 2:
                {
                    NewPos = RandomNumber.GenerateRandom(0, 2) == 1
                        ? new Point(Position.X - 1, Position.Y)
                        : new Point(Position.X + 1, Position.Y);
                    break;
                }
                case 3:
                {
                    NewPos = RandomNumber.GenerateRandom(0, 2) == 1
                        ? new Point(Position.X, Position.Y - 1)
                        : new Point(Position.X, Position.Y + 1);
                    break;
                }
                case 4:
                {
                    NewPos = new Point(Position.X, Position.Y - 1);
                    break;
                }
                case 5:
                {
                    NewPos = new Point(Position.X + 1, Position.Y);
                    break;
                }
                case 6:
                {
                    NewPos = new Point(Position.X, Position.Y + 1);
                    break;
                }
                case 7:
                {
                    NewPos = new Point(Position.X - 1, Position.Y);
                    break;
                }
            }

            return NewPos;
        }
    }
}