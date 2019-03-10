#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    public class AddFavouriteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
                return;

            var RoomId = Packet.PopInt();

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(RoomId);

            if (Data == null || Session.GetHabbo().FavoriteRooms.Count >= 30 ||
                Session.GetHabbo().FavoriteRooms.Contains(RoomId))
                return;

            Session.GetHabbo().FavoriteRooms.Add(RoomId);
            Session.SendMessage(new UpdateFavouriteRoomComposer(RoomId, true));

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("INSERT INTO user_favorites (user_id,room_id) VALUES (" + Session.GetHabbo().Id + "," +
                                  RoomId + ")");
            }
        }
    }
}