#region

using System.Collections;
using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class KickUserBox : IWiredItem, IWiredCycle
    {
        private readonly Queue _toKick;

        public KickUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            _toKick = new Queue();

            SetItems.Clear();
        }

        public double TickCount { get; set; }
        public double Delay { get; set; }

        public bool OnCycle()
        {
            if (Instance == null)
                return false;

            if (_toKick.Count == 0)
            {
                TickCount = 3;
                return true;
            }

            lock (_toKick.SyncRoot)
            {
                while (_toKick.Count > 0)
                {
                    var Player = (Habbo) _toKick.Dequeue();
                    if (Player == null || !Player.InRoom || Player.CurrentRoom != Instance)
                        continue;

                    Instance.GetRoomUserManager().RemoveUserFromRoom(Player.GetClient(), true, false);
                }
            }
            TickCount = 3;
            return true;
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectKickUser;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            var Unknown = Packet.PopInt();
            var Message = Packet.PopString();

            StringData = Message;
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length != 1)
                return false;

            var Player = (Habbo) Params[0];
            if (Player == null)
                return false;

            if (TickCount <= 0)
                TickCount = 3;

            lock (_toKick.SyncRoot)
            {
                if (!_toKick.Contains(Player))
                {
                    var User = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
                    if (User == null)
                        return false;

                    if (Player.GetPermissions().HasRight("mod_tool") || Instance.OwnerId == Player.Id)
                    {
                        Player.GetClient()
                            .SendMessage(new WhisperComposer(User.VirtualId, "Você não pode ser expulso.", 0, 34));
                        return false;
                    }

                    _toKick.Enqueue(Player);
                    Player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, StringData, 0, 0));
                }
            }
            return true;
        }
    }
}