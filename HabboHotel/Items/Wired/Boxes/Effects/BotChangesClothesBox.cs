#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class BotChangesClothesBox : IWiredItem
    {
        public BotChangesClothesBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectBotChangesClothesBox;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var BotConfiguration = Packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            StringData = BotConfiguration;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;


            var Stuff = StringData.Split('\t');
            if (Stuff.Length != 2)
                return false; //This is important, incase a cunt scripts.

            var Username = Stuff[0];

            var User = Instance.GetRoomUserManager().GetBotByName(Username);
            if (User == null)
                return false;

            var Figure = Stuff[1];

            var UserChangeComposer = new ServerPacket(ServerPacketHeader.UserChangeMessageComposer);
            UserChangeComposer.WriteInteger(User.VirtualId);
            UserChangeComposer.WriteString(Figure);
            UserChangeComposer.WriteString("M");
            UserChangeComposer.WriteString(User.BotData.Motto);
            UserChangeComposer.WriteInteger(0);
            Instance.SendMessage(UserChangeComposer);

            User.BotData.Look = Figure;
            User.BotData.Gender = "M";

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `bots` SET `look` = @look, `gender` = '" + User.BotData.Gender +
                                  "' WHERE `id` = '" + User.BotData.Id + "' LIMIT 1");
                dbClient.AddParameter("look", User.BotData.Look);
                dbClient.RunQuery();
            }

            return true;
        }
    }
}