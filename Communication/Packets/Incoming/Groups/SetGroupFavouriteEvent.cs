#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class SetGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
                return;

            var GroupId = Packet.PopInt();
            if (GroupId == 0)
                return;

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            Session.GetHabbo().GetStats().FavouriteGroupId = Group.Id;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `user_stats` SET `groupid` = " +
                                  Session.GetHabbo().GetStats().FavouriteGroupId + " WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
            }

            if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoom != null)
            {
                Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
                if (Group != null)
                {
                    Session.GetHabbo().CurrentRoom.SendMessage(new HabboGroupBadgesComposer(Group));

                    var User =
                        Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                    if (User != null)
                        Session.GetHabbo()
                            .CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, Group,
                                User.VirtualId));
                }
            }
            else
            {
                Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
        }
    }
}