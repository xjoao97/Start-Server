#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes
{
    internal class UserFastWalkBox : IWiredItem
    {
        public UserFastWalkBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectUserFastWalk;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
         
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            var Player = (Habbo)Params[0];
            if (Player?.GetClient() == null)
                return false;

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
                return false;

            User.FastWalking = !User.FastWalking;
            return true;
        }
    }
}