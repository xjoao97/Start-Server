#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class GiveUserBadgeBox : IWiredItem
    {
        public GiveUserBadgeBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectGiveUserBadge;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Badge = Packet.PopString();

            StringData = Badge;
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

            if (Player.GetBadgeComponent().HasBadge(StringData))
            {
                Player.GetClient()
                    .SendMessage(new WhisperComposer(User.VirtualId,
                        "Opa, você já recebeu este emblema!", 0, 34));
            }
            else
            {
                Player.GetBadgeComponent().GiveBadge(StringData, true, Player.GetClient());
                Player.GetClient().SendMessage(new RoomNotificationComposer("", "Você recebeu um emblema.", "generic","","", true));
            }

            return true;
        }
    }
}