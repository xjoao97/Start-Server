#region

using System.Collections.Generic;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.HabboHotel.Rooms.Trading
{
    public class TradeUser
    {
        private readonly int RoomId;
        public List<Item> OfferedItems;
        public int UserId;

        public TradeUser(int UserId, int RoomId)
        {
            this.UserId = UserId;
            this.RoomId = RoomId;
            HasAccepted = false;
            OfferedItems = new List<Item>();
        }

        public bool HasAccepted { get; set; }

        public RoomUser GetRoomUser()
        {
            Room Room;

            return !OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room) ? null : Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
        }

        public GameClient GetClient() => OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
    }
}