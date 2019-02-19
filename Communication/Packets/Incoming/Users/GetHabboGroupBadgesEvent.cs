#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class GetHabboGroupBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var Badges = new Dictionary<int, string>();
            foreach (Group Group in Room.GetRoomUserManager().GetRoomUsers().ToList().Where(User => !User.IsBot && !User.IsPet && User.GetClient() != null && User.GetClient().GetHabbo() != null && User.GetClient().GetHabbo().GetStats().FavouriteGroupId != 0 && !Badges.ContainsKey(User.GetClient().GetHabbo().GetStats().FavouriteGroupId)).Select(User => OblivionServer.GetGame().GetGroupManager().TryGetGroup(User.GetClient().GetHabbo().GetStats().FavouriteGroupId)).Where(Group => Group != null).Where(Group => !Badges.ContainsKey(Group.Id)))
            {
                Badges.Add(Group.Id, Group.Badge);
            }

            if (Session.GetHabbo().GetStats().FavouriteGroupId > 0)
            {

               var Group = OblivionServer.GetGame()
                    .GetGroupManager()
                    .TryGetGroup(Session.GetHabbo().GetStats().FavouriteGroupId);
                if (Group != null)
                    if (!Badges.ContainsKey(Group.Id))
                        Badges.Add(Group.Id, Group.Badge);
            }

            Room.SendMessage(new HabboGroupBadgesComposer(Badges));
            Session.SendMessage(new HabboGroupBadgesComposer(Badges));
        }
    }
}