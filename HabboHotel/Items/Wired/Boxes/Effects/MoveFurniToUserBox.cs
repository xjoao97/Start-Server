#region

using System;
using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MoveFurniToUserBox : IWiredItem, IWiredCycle
    {
        private double _delay;
        private double _next;
        private bool Requested;

        public MoveFurniToUserBox(Room Instance, Item Item)
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
            var time = OblivionServer.GetUnixTimestamp();           
            if (_next <= time)
            {
                foreach (
                    var Item in
                    SetItems.Values.ToList()
                        .Where(Item => Item != null)
                        .Where(Item => Instance.GetRoomItemHandler().GetFloor.Contains(Item)))
                {
                    Item toRemove;

                    if (Instance.GetWired().OtherBoxHasItem(this, Item.Id))
                        SetItems.TryRemove(Item.Id, out toRemove);

                    var Point = Instance.GetGameMap().GetChaseMovement(Item);

                    Instance.GetWired().onUserFurniCollision(Instance, Item);

                    if (!Instance.GetGameMap().ItemCanMove(Item, Point))
                        continue;

                    if (Instance.GetGameMap().CanRollItemHere(Point.X, Point.Y) &&
                        !Instance.GetGameMap().SquareHasUsers(Point.X, Point.Y))
                    {
                        var NewZ = Item.GetZ;
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

        public WiredBoxType Type => WiredBoxType.EffectMoveFurniToNearestUser;

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

                if (SelectedItem != null && !Instance.GetWired().OtherBoxHasItem(this, SelectedItem.Id))
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
            }

            var Delay = Packet.PopInt();
            this.Delay = Delay;
        }

        public bool Execute(params object[] Params)
        {
            if (SetItems.Count == 0)
                return false;


            if (_next < 1 || _next < OblivionServer.GetUnixTimestamp())
            {
                _next = OblivionServer.GetUnixTimestamp() + Delay;
            }
            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }
            return true;
        }
    }
}