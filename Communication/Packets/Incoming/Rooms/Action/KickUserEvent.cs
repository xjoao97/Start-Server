#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class KickUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.CheckRights(Session) && Room.WhoCanKick != 2 && Room.Group == null)
                return;

            if (Room.Group != null && !Room.CheckRights(Session, false, true))
                return;

            var UserId = Packet.PopInt();
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
            if (User == null || User.IsBot)
                return;

            //Cannot kick owner or moderators.
            if (Room.CheckRights(User.GetClient(), true) || User.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") &&
                User.GetClient().GetHabbo().Rank > Session.GetHabbo().Rank)
                return;

            Room.GetRoomUserManager().RemoveUserFromRoom(User.GetClient(), true, true);
            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModKickSeen", 1);
        }
    }
}