#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Action
{
    internal class GiveRoomScoreEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (Session.GetHabbo().RatedRooms.Contains(Room.RoomId) || Room.CheckRights(Session, true))
                return;

            var Rating = Packet.PopInt();
            switch (Rating)
            {
                case -1:

                    Room.Score--;
                    Room.RoomData.Score--;
                    break;

                case 1:

                    Room.Score++;
                    Room.RoomData.Score++;
                    break;

                default:

                    return;
            }


            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE rooms SET score = '" + Room.Score + "' WHERE id = '" + Room.RoomId +
                                  "' LIMIT 1");
            }

            Session.GetHabbo().RatedRooms.Add(Room.RoomId);
            Session.SendMessage(new RoomRatingComposer(Room.Score,
                !(Session.GetHabbo().RatedRooms.Contains(Room.RoomId) || Room.CheckRights(Session, true))));
        }
    }
}