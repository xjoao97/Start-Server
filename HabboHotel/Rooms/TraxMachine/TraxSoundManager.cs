#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Rooms.TraxMachine
{
    public class TraxSoundManager
    {
        private readonly ILog Log = LogManager.GetLogger("Oblivion.HabboHotel.Rooms.TraxMachine");
        public List<TraxMusicData> Songs;

        public void Init()
        {
            Songs = new List<TraxMusicData>();

            Flush();
            Log.Info("Habbo Trax Machine Manager -> Sounds Loaded Sucessfully [ " + Songs.Count + " Songs ]");
        }

        public void Flush()
        {
            Songs.Clear();

            DataTable table;
            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.RunQuery("SELECT * FROM jukebox_songs_data");
                table = adap.GetTable();
            }

            foreach (DataRow row in table.Rows)
                Songs.Add(TraxMusicData.Parse(row));
        }


        public TraxMusicData GetMusic(int id) => Songs?.FirstOrDefault(item => item.Id == id);
    }
}