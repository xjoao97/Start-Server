#region

using System;
using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes
{
    internal class EnableUserHanditemBox : IWiredItem
    {
        public EnableUserHanditemBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectEnableUserHandItem;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var OwnerOnly = Packet.PopInt();
            var Effect = Packet.PopString();

            BoolData = OwnerOnly == 1;
            StringData = Effect;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            var Player = (Habbo)Params[0];
            var Effect = Convert.ToInt32(StringData);
            if (Player?.GetClient() == null || Effect < 0)
                return false;

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
                return false;

            User.CarryItem(Effect);
            return true;
        }
    }
}