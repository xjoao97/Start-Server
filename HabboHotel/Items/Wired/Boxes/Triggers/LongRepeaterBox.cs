#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class LongRepeaterBox : IWiredItem, IWiredCycle
    {
        private double _delay = 20 * 5000;

        public LongRepeaterBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                TickCount = value;
            }
        }

        public double TickCount { get; set; }

        public bool OnCycle()
        {
            var Success = false;
            ICollection<RoomUser> Avatars = Instance.GetRoomUserManager().GetRoomUsers().ToList();
            var Effects = Instance.GetWired().GetEffects(this);
            var Conditions = Instance.GetWired().GetConditions(this);

            foreach (var Condition in Conditions.ToList())
            {
                foreach (var Avatar in Avatars.ToList())
                {
                    if (Avatar?.GetClient() == null || Avatar.GetClient().GetHabbo() == null)
                        continue;

                    if (!Condition.Execute(Avatar.GetClient().GetHabbo()))
                        continue;

                    Success = true;
                }

                if (!Success)
                    return false;

                Success = false;
                Instance.GetWired().OnEvent(Condition.Item);
            }


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
                foreach (var Effect in Effects.ToList().Where(Effect => Effect.Execute()))
                    Instance?.GetWired().OnEvent(Effect.Item);
            }

            TickCount = Delay;

            return true;
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.TriggerLongRepeat;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Delay = Packet.PopInt() * 5000;

            this.Delay = Delay / 500;
            TickCount = Delay / 5000;
            StringData = TickCount + ";";
        }

        public bool Execute(params object[] Params) => true;
    }
}