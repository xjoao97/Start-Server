/*#region

using System.Collections.Concurrent;
using System.Linq;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class TimerBox : IWiredItem, IWiredCycle
    {
        private int _delay;

        public TimerBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

<<<<<<< HEAD
        public double Delay
=======
        public int Delay
>>>>>>> origin/master
        {
            get { return _delay; }
            set
            {
                _delay = value;
                TickCount = value;
            }
        }

        public int TickCount { get; set; }

        public bool OnCycle()
        {
            var Success = false;
            var Effects = Instance.GetWired().GetEffects(this).Take(10).ToList();
            var Conditions = Instance.GetWired().GetConditions(this).Take(35).ToList();

            if (Conditions.Count > 0)
            {
                foreach (var Condition in Conditions)
                {
                    if (!Condition.Execute(null))
                    {
                        return false;
                    }
                    Instance.GetWired().OnEvent(Condition.Item);
                }
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

        public WiredBoxType Type => WiredBoxType.TriggerRepeat;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var Delay = Packet.PopInt();

            this.Delay = Delay;
            TickCount = Delay;
        }

        public bool Execute(params object[] Params) => true;
    }
}*/