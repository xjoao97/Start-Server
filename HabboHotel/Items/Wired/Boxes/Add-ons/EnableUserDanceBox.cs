#region

using System;
using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes
{
    internal class EnableUserDanceBox : IWiredItem
    {
        public EnableUserDanceBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectEnableUserDance;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var OwnerOnly = Packet.PopInt();
            var Dance = Packet.PopString();

            BoolData = OwnerOnly == 1;
            StringData = Dance;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            var Player = (Habbo)Params[0];
            var Dance = Convert.ToInt32(StringData);
            if (Player?.GetClient() == null || Dance < 0 || Dance > 4)
                return false;

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
                return false;

            User.ApplyEffect(0);
            User.DanceId = Dance;
            Player.CurrentRoom.SendMessage(new DanceComposer(User, Dance));
            return true;
        }
    }
}