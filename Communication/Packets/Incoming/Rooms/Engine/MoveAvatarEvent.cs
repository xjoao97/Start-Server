#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class MoveAvatarEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || !User.CanWalk)
                return;

            var MoveX = Packet.PopInt();
            var MoveY = Packet.PopInt();

            if (MoveX == User.X && MoveY == User.Y)
                return;

            if (User.RidingHorse)
            {
                var Horse = Room.GetRoomUserManager().GetRoomUserByVirtualId(User.HorseID);
                Horse?.MoveTo(MoveX, MoveY);
            }

            User.MoveTo(MoveX, MoveY);
        }
    }
}