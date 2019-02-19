﻿#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class IsNotGroupMemberBox : IWiredItem
    {
        public IsNotGroupMemberBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.ConditionIsNotGroupMember;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Unknown2 = Packet.PopString();
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0)
                return false;

            var Player = (Habbo) Params[0];
            if (Player == null)
                return false;

            if (Instance.RoomData.Group == null)
                return false;

            return !Instance.RoomData.Group.IsMember(Player.Id);
        }
    }
}