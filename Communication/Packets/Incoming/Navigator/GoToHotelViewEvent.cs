#region

using Oblivion.Communication.Packets.Outgoing.LandingView;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class GoToHotelViewEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;


            if (!Session.GetHabbo().InRoom) return;
            Room OldRoom;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                return;

            if (OldRoom.GetRoomUserManager() != null)
                OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, true);

            Session.SendMessage(new HallOfFameComposer(OblivionServer.GetGame().GetLandingManager().GetHallOfFameUsers()));
        }
    }
}