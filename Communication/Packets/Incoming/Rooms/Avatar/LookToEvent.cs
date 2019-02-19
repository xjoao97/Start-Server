#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Avatar
{
    internal class LookToEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (User.IsAsleep)
                return;

            User.UnIdle();

            var X = Packet.PopInt();
            var Y = Packet.PopInt();

            if (X == User.X && Y == User.Y || User.IsWalking || User.RidingHorse)
                return;

            var Rot = Rotation.Calculate(User.X, User.Y, X, Y);

            User.SetRot(Rot, false);
            User.UpdateNeeded = true;

            if (User.RidingHorse)
            {
                var Horse = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByVirtualId(User.HorseID);
                if (Horse != null)
                {
                    Horse.SetRot(Rot, false);
                    Horse.UpdateNeeded = true;
                }
            }
        }
    }
}