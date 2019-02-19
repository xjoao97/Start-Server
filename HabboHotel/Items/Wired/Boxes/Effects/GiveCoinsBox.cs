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
    internal class GiveCoinsBox : IWiredItem
    {
        public GiveCoinsBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectGiveUserCredits;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Often = Packet.PopInt();
            var Unique = Packet.PopInt() == 1;
            var Limit = Packet.PopInt();
            var Often_No = Packet.PopInt();
            var Reward = Packet.PopString();

            BoolData = Unique;
            StringData = Reward + "-" + Often + "-" + Limit;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            var Owner = OblivionServer.GetHabboById(Item.UserID);
            if (Owner == null || !Owner.GetPermissions().HasRight("room_item_wired_rewards"))
                return false;

            var Player = (Habbo) Params[0];
            if (Player?.GetClient() == null)
                return false;

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            var amountLeft = int.Parse(StringData.Split('-')[2]);
            var often = int.Parse(StringData.Split('-')[1]);
            var unique = BoolData;

            var premied = false;

            if (amountLeft == 1)
            {
                Player.GetClient().SendNotification("Os prêmios acabaram, volte mais tarde.");
                return true;
            }

            foreach (var quantity in from dataStr in StringData.Split('-')[0].Split(';')
                select dataStr.Split(',')
                into dataArray
                let quantity = dataArray[1]
                let percentage = int.Parse(dataArray[2])
                let random = OblivionServer.GetRandomNumber(0, 100)
                where unique || percentage >= random
                select quantity)
            {
                premied = true;

                Player.GetClient().GetHabbo().Credits += Convert.ToInt32(quantity);
                Player.GetClient().GetHabbo().UpdateCreditsBalance(Player.GetClient());
            }

            if (!premied)
            {
                Player.GetClient().SendNotification("Sorte da próxima vez. :(");
            }
            else if (amountLeft > 1)
            {
                amountLeft--;
                StringData.Split('-')[2] = amountLeft.ToString();
            }

            return true;
        }
    }
}