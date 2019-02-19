#region

using System;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class BanUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (Room.WhoCanBan == 0 && !Room.CheckRights(Session, true) && Room.Group == null ||
                Room.WhoCanBan == 1 && !Room.CheckRights(Session) && Room.Group == null ||
                Room.Group != null && !Room.CheckRights(Session, false, true))
                return;

            var UserId = Packet.PopInt();
            var RoomId = Packet.PopInt();
            var R = Packet.PopString();

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToInt32(UserId));
            if (User == null || User.IsBot)
                return;

            if (Room.OwnerId == UserId)
                return;

            if (User.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            long Time = 0;
            if (R.ToLower().Contains("hour"))
                Time = 3600;
            else if (R.ToLower().Contains("day"))
                Time = 86400;
            else if (R.ToLower().Contains("perm"))
                Time = 78892200;

            Room.AddBan(UserId, Time);
            Room.GetRoomUserManager().RemoveUserFromRoom(User.GetClient(), true, true);

            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModBanSeen", 1);
        }
    }
}