#region

using System;

#endregion

namespace Oblivion.HabboHotel.Items
{
    public static class ItemHopperFinder
    {
        public static int GetAHopper(int CurRoom)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_id FROM items_hopper WHERE room_id <> @room ORDER BY room_id ASC LIMIT 1");
                dbClient.AddParameter("room", CurRoom);
                var RoomId = dbClient.getInteger();
                return RoomId;
            }
        }

        public static int GetHopperId(int NextRoom)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT hopper_id FROM items_hopper WHERE room_id = @room LIMIT 1");
                dbClient.AddParameter("room", NextRoom);
                var Row = dbClient.getString();

                return Row == null ? 0 : Convert.ToInt32(Row);
            }
        }
    }
}