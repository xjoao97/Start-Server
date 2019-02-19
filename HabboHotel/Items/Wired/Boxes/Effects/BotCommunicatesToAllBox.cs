#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class BotCommunicatesToAllBox : IWiredItem
    {
        public BotCommunicatesToAllBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectBotCommunicatesToAllBox;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var ChatMode = Packet.PopInt();
            var ChatConfig = Packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            //this.StringData = ChatConfig.Replace('\t', ';') + ";" + ChatMode;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            var User = Instance.GetRoomUserManager().GetBotByName(StringData);
            if (User == null)
                return false;

            if (BoolData)
            {
                Instance.SendMessage(new WhisperComposer(User.VirtualId, "[Tent Chat] " + ItemsData, 0, 33));
                return true;
            }
            User.Chat(ItemsData, false, 33);
            return true;
        }
    }
}