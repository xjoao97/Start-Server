using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    class GiveUserDiamondsBox : IWiredItem
    {
        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type { get { return WiredBoxType.EffectGiveUserDiamonds; } }

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public GiveUserDiamondsBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            string Diamonds = Packet.PopString();

            this.StringData = Diamonds;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            Habbo Owner = OblivionServer.GetHabboById(Item.UserID);
            if (Owner == null || !Owner.GetPermissions().HasRight("room_item_wired_rewards"))
                return false;

            Habbo Player = (Habbo)Params[0];
            if (Player == null || Player.GetClient() == null)
                return false;

            RoomUser User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
                return false;

            if (String.IsNullOrEmpty(StringData))
                return false;

            int Amount;
            Amount = Convert.ToInt32(StringData);
            if (Amount > 50)
            {
                Player.GetClient().SendWhisper("La cantidad de Diamantes pasa de los limites.");
                return false;
            }
            else
            {
                Player.GetClient().GetHabbo().Diamonds += Amount;
                Player.GetClient().SendMessage(new ActivityPointsComposer(Player.GetClient().GetHabbo().Duckets, Player.GetClient().GetHabbo().Diamonds, Player.GetClient().GetHabbo().GOTWPoints));
                Player.GetClient().SendMessage(RoomNotificationComposer.SendBubble("diamonds", "" + Player.GetClient().GetHabbo().Username + ", acabas de recibir " + Amount + " Diamante(s).", ""));

            }

            return true;
        }
    }
}