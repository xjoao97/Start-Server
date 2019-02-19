#region

using System.Data;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    internal class NuxPassed
    {
        public void NuxPassed1()
        {
            DataRow Row = null;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                // dbClient.SetQuery("SELECT * FROM users WHERE ID ='" + OblivionServer.GetHabboById().AccountCreated + "'AND nux_passed = '1' LIMIT 1");
                // dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '1' AND `id` = '" + Id + "' LIMIT 1");
                Row = dbClient.getRow();
            }
        }
    }
}