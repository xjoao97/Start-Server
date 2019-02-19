#region

using System.Collections.Concurrent;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Games.Teams;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class RemoveActorFromTeamBox : IWiredItem
    {
        public RemoveActorFromTeamBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;

            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectRemoveActorFromTeam;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public void HandleSave(ClientPacket Packet)
        {
            var Unknown = Packet.PopInt();
        }

        public bool Execute(params object[] Params)
        {
            if (Params.Length == 0 || Instance == null)
                return false;

            var Player = (Habbo) Params[0];
            if (Player == null)
                return false;

            var User = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
            if (User == null)
                return false;

            if (User.Team != TEAM.NONE)
            {
                var Team = Instance.GetTeamManagerForFreeze();
                if (Team != null)
                {
                    Team.OnUserLeave(User);

                    User.Team = TEAM.NONE;

                    if (User.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                }
            }
            return true;
        }
    }
}