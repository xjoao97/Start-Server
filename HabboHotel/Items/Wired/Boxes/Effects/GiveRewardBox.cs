#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class GiveRewardBox : IWiredItem
    {
        public GiveRewardBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectGiveReward;

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

            foreach (var dataStr in StringData.Split('-')[0].Split(';'))
            {
                var dataArray = dataStr.Split(',');

                var isbadge = dataArray[0] == "0";
                var code = dataArray[1];
                var percentage = int.Parse(dataArray[2]);

                var random = OblivionServer.GetRandomNumber(0, 100);

                if (!unique && percentage < random)

                    continue;

                premied = true;


                if (isbadge)
                {
                    if (Player.GetBadgeComponent().HasBadge(code))
                    {
                        Player.GetClient()
                            .SendMessage(new WhisperComposer(User.VirtualId,
                                "Oops, parece que você já tem este emblema.", 0, User.LastBubble));
                    }

                    else
                    {
                        Player.GetBadgeComponent().GiveBadge(code, true, Player.GetClient());
                        Player.GetClient().SendMessage(new RoomNotificationComposer("", "Você recebeu um emblema.", "generic", "", "", true));
                    }
                }
                else
                {
                    ItemData ItemData = OblivionServer.GetGame().GetItemManager().GetItem(int.Parse(code));

                    if (ItemData == null)
                    {
                        Player.GetClient()
                            .SendMessage(new WhisperComposer(User.VirtualId, "ITEM ID não existe: " + code, 0,
                                User.LastBubble));
                        return false;
                    }

                    var Itemc = ItemFactory.CreateSingleItemNullable(ItemData, Player.GetClient().GetHabbo(), "", "");


                    if (Itemc != null)
                    {
                        Player.GetClient().GetHabbo().GetInventoryComponent().TryAddItem(Itemc);
                        Player.GetClient().SendMessage(new FurniListNotificationComposer(Itemc.Id, 1));
                        Player.GetClient().SendMessage(new PurchaseOkComposer());
                        Player.GetClient().SendMessage(new FurniListAddComposer(Itemc));
                        Player.GetClient().SendMessage(new FurniListUpdateComposer());
                        Player.GetClient().SendNotification("Você acaba de receber um mobi, confira o seu inventário.");
                    }
                }
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