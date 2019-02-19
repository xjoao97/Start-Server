﻿#region

using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class GameEndsBox : IWiredItem
    {
        public GameEndsBox(Room Instance, Item Item)
        {
            this.Item = Item;
            this.Instance = Instance;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.TriggerGameEnds;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
        }

        public bool Execute(params object[] Params)
        {
            var Effects = Instance.GetWired().GetEffects(this);
            var Conditions = Instance.GetWired().GetConditions(this);

            foreach (var Condition in Conditions)
                Instance.GetWired().OnEvent(Condition.Item);

            //Check the ICollection to find the random addon effect.
            var HasRandomEffectAddon = Effects.Where(x => x.Type == WiredBoxType.AddonRandomEffect).ToList().Any();
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
                foreach (var Effect in Effects)
                {
                    foreach (
                        var User in
                        Instance.GetRoomUserManager()
                            .GetRoomUsers()
                            .ToList()
                            .Where(User => User?.GetClient() != null && User.GetClient().GetHabbo() != null))
                        Effect.Execute(User.GetClient().GetHabbo());

                    Instance.GetWired().OnEvent(Effect.Item);
                }
            }

            return true;
        }
    }
}