#region

using System;
using System.Data;

#endregion

namespace Oblivion.HabboHotel.Items.Data.Toner
{
    public class TonerData
    {
        public int Enabled;
        public int Hue;
        public int ItemId;
        public int Lightness;
        public int Saturation;

        public TonerData(int Item)
        {
            ItemId = Item;

            DataRow Row;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + ItemId +
                                  " LIMIT 1");
                Row = dbClient.getRow();
            }

            if (Row == null)
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("INSERT INTO `room_items_toner` VALUES (" + ItemId + ",'0',0,0,0)");
                    dbClient.SetQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + ItemId +
                                      " LIMIT 1");
                    Row = dbClient.getRow();
                }

            Enabled = int.Parse(Row[0].ToString());
            Hue = Convert.ToInt32(Row[1]);
            Saturation = Convert.ToInt32(Row[2]);
            Lightness = Convert.ToInt32(Row[3]);
        }
    }
}