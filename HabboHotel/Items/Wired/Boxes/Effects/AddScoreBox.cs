using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class AddScoreBox : IWiredItem, IWiredCycle
    {
        private double _delay;

        private readonly Queue _queue;

        public AddScoreBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();

            _queue = new Queue();
            TickCount = Delay;
        }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                TickCount = value + 1;
            }
        }

        public double TickCount { get; set; }

        public bool OnCycle()
        {
            if (_queue.Count == 0)
            {
                _queue.Clear();
                TickCount = Delay;
                return true;
            }

            while (_queue.Count > 0)
            {
                var Player = (Habbo) _queue.Dequeue();
                if (Player == null || Player.CurrentRoom != Instance)
                    continue;

                TeleportUser(Player);
            }

            TickCount = Delay;
            return true;
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectAddScore;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
            var score = Packet.PopInt();
            var times = Packet.PopInt();
            var Unknown2 = Packet.PopString();
            var Unknown3 = Packet.PopInt();
            var Delay = Packet.PopInt();

            this.Delay = Delay;
            StringData = score + ";" + times;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            var Player = (Habbo) Params[0];

            if (Player == null)
                return false;

            _queue.Enqueue(Player);
            return true;
        }

        private void TeleportUser(Habbo Player)
        {
            var User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
            if (User == null || string.IsNullOrEmpty(StringData))
                return;

            var Instance = Player.CurrentRoom;

            var mScore = Convert.ToInt32(StringData.Split(';')[0]) * Convert.ToInt32(StringData.Split(';')[1]);
            if (mScore < 0)
                return;

            if (Instance != null && !User.IsBot)
            {
                Instance.GetRoomItemHandler().Usedwiredscorebord = true;

                if (Instance.WiredScoreFirstBordInformation.Count == 3)
                    Instance.GetRoomItemHandler().ScorebordChangeCheck();

                if (Instance.WiredScoreBordDay != null && Instance.WiredScoreBordMonth != null &&
                    Instance.WiredScoreBordWeek != null)
                {
                    var username = User.GetClient().GetHabbo().Username;

                    int currentscore;
                    KeyValuePair<int, string> item;
                    KeyValuePair<int, string> newkey;
//                   
                    if (!Instance.WiredScoreBordDay.ContainsKey(User.UserId))
                    {
                        Instance.WiredScoreBordDay.Add(User.UserId, new KeyValuePair<int, string>(mScore, username));
                    }
                    else
                    {
                        item = Instance.WiredScoreBordDay[User.UserId];
                        currentscore = item.Key + mScore;

                        newkey = new KeyValuePair<int, string>(currentscore, username);
                        Instance.WiredScoreBordDay[User.UserId] = newkey;
                    }
//                   
                    if (!Instance.WiredScoreBordWeek.ContainsKey(User.UserId))
                    {
                        Instance.WiredScoreBordWeek.Add(User.UserId, new KeyValuePair<int, string>(mScore, username));
                    }
                    else
                    {
                        item = Instance.WiredScoreBordWeek[User.UserId];
                        currentscore = item.Key + mScore;

                        newkey = new KeyValuePair<int, string>(currentscore, username);
                        Instance.WiredScoreBordWeek[User.UserId] = newkey;
                    }

                    if (!Instance.WiredScoreBordMonth.ContainsKey(User.UserId))
                    {
                        Instance.WiredScoreBordMonth.Add(User.UserId,
                            new KeyValuePair<int, string>(mScore, username));
                    }
                    else
                    {
                        item = Instance.WiredScoreBordMonth[User.UserId];
                        currentscore = item.Key + mScore;
                        newkey = new KeyValuePair<int, string>(currentscore, username);
                        Instance.WiredScoreBordMonth[User.UserId] = newkey;
                    }
                }
                Task.Factory.StartNew(() =>
                {
                    Instance.GetRoomItemHandler().UpdateWiredScoreBord();
                    User.GetClient().SendWhisper("Você ganhou " + mScore + " pontos de classificação!");
                });
            }
        }
    }
}