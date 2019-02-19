#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class RemoveGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.GetHabbo().GetStats().FavouriteGroupId = 0;

            if (Session.GetHabbo().InRoom)
            {
                var User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User != null)
                    Session.GetHabbo()
                        .CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(Session.GetHabbo().Id, null,
                            User.VirtualId));
                Session.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
            else
            {
                Session.SendMessage(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
        }
    }
}