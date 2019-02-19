#region

using System.Data;

#endregion

namespace Oblivion.HabboHotel.Rooms.TraxMachine
{
    public class TraxMusicData
    {
        public string Artist;
        public string CodeName;
        public string Data;
        public int Id;
        public int Length;
        public string Name;

        public TraxMusicData(int id, string code, string name, string art, string data, int len)
        {
            Id = id;
            CodeName = code;
            Name = name;
            Artist = art;
            Data = data;
            Length = len;
        }

        public static TraxMusicData Parse(DataRow row)
            => new TraxMusicData(int.Parse(row["id"].ToString()), row["codename"].ToString(), row["name"].ToString(),
                row["artist"].ToString(), row["song_data"].ToString(), int.Parse(row["length"].ToString()));
    }
}