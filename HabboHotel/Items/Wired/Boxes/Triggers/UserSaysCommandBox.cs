#region

using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Chat.Commands;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class UserSaysCommandBox : IWiredItem
    {
        public UserSaysCommandBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            StringData = "";
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.TriggerUserSaysCommand;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var OwnerOnly = Packet.PopInt();
            var Message = Packet.PopString();

            BoolData = OwnerOnly == 1;
            StringData = Message;
        }

        public bool Execute(params object[] Params)
        {
            var Player = (Habbo) Params[0];
            if (Player?.CurrentRoom == null || !Player.InRoom)
                return false;

            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Username);
            if (User == null)
                return false;

            if (BoolData && Instance.OwnerId != Player.Id || string.IsNullOrWhiteSpace(StringData))
                return false;

            IChatCommand ChatCommand;
            if (
                !OblivionServer.GetGame()
                    .GetChatManager()
                    .GetCommands()
                    .TryGetCommand(StringData.Replace(":", "").ToLower(), out ChatCommand))
                return false;

            if (Player.IChatCommand == ChatCommand)
            {
                Player.WiredInteraction = true;
                var Effects = Instance.GetWired().GetEffects(this);
                var Conditions = Instance.GetWired().GetConditions(this);

                foreach (var Condition in Conditions.ToList())
                {
                    if (!Condition.Execute(Player))
                        return false;

                    Instance.GetWired().OnEvent(Condition.Item);
                }

                Player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, StringData, 0, 0));
                //Check the ICollection to find the random addon effect.
                var HasRandomEffectAddon =
                    Effects.Where(x => x.Type == WiredBoxType.AddonRandomEffect).ToList().Any();
                if (HasRandomEffectAddon)
                {
                    //Okay, so we have a random addon effect, now lets get the IWiredItem and attempt to execute it.
                    var RandomBox = Effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
                    if (!RandomBox.Execute())
                        return false;

                    //Success! Let's get our selected box and continue.
                    var SelectedBox = Instance.GetWired().GetRandomEffect(Effects.ToList());
                    if (!SelectedBox.Execute())
                        return false;

                    //Woo! Almost there captain, now lets broadcast the update to the room instance.
                    Instance?.GetWired().OnEvent(RandomBox.Item);
                    Instance?.GetWired().OnEvent(SelectedBox.Item);
                }
                else
                {
                    foreach (var Effect in Effects.ToList())
                    {
                        if (!Effect.Execute(Player))
                            return false;

                        Instance.GetWired().OnEvent(Effect.Item);
                    }
                }
                return true;
            }

            return false;
        }
    }
}