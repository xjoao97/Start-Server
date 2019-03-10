#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class RoomData
    {
        private RoomModel _mModel;
        public RoomAccess Access;
        public int AllowPets;
        public int AllowPetsEating;
        internal List<string> BlockedCommands;
        public int Category;
        public int ChatDistance;
        public int ChatMode;
        public int ChatSize;
        public int ChatSpeed;
        public string Description;
        public int ExtraFlood;
        public string Floor;
        public int FloorThickness;
        public Group Group;
        public int Hidewall;
        public int Id;
        public string Landscape;
        public string ModelName;
        public string Name;
        public int OwnerId;
        public string OwnerName;
        public string Password;

        public int RoomBlockingEnabled;
        public bool RoomForSale = false;
        public int RoomSaleCost = 0;
        public string RoomSaleType = "";
        public int Score;
        public List<string> Tags;
        public int TradeSettings; //Default = 2;
        public string Type;
        public int UsersMax;
        public int UsersNow;
        public string Wallpaper;
        public int WallThickness;
        public int WhoCanBan;
        public int WhoCanKick;
        public int WhoCanMute;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordDay;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordMonth;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordWeek;
        public List<int> WiredScoreFirstBordInformation = new List<int>();

        public RoomPromotion Promotion { get; set; }

        public bool HasActivePromotion => Promotion != null;

        public RoomModel Model => _mModel ?? (_mModel = OblivionServer.GetGame().GetRoomManager().GetModel(ModelName));

        public void Fill(DataRow row)
        {
            Id = Convert.ToInt32(row["id"]);
            Name = Convert.ToString(row["caption"]);
            Description = Convert.ToString(row["description"]);
            Type = Convert.ToString(row["roomtype"]);
            OwnerId = Convert.ToInt32(row["owner"]);
            BlockedCommands = new List<string>();
            OwnerName = "";
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @owner LIMIT 1");
                dbClient.AddParameter("owner", OwnerId);
                var result = dbClient.GetString();
                if (!string.IsNullOrEmpty(result))
                    OwnerName = result;
            }

            Access = RoomAccessUtility.ToRoomAccess(row["state"].ToString().ToLower());

            Category = Convert.ToInt32(row["category"]);
            UsersNow = !string.IsNullOrEmpty(row["users_now"].ToString()) ? Convert.ToInt32(row["users_now"]) : 0;
            UsersMax = Convert.ToInt32(row["users_max"]);
            ModelName = Convert.ToString(row["model_name"]);
            Score = Convert.ToInt32(row["score"]);
            Tags = new List<string>();
            AllowPets = Convert.ToInt32(row["allow_pets"].ToString());
            AllowPetsEating = Convert.ToInt32(row["allow_pets_eat"].ToString());
            RoomBlockingEnabled = Convert.ToInt32(row["room_blocking_disabled"].ToString());
            Hidewall = Convert.ToInt32(row["allow_hidewall"].ToString());
            Password = Convert.ToString(row["password"]);
            Wallpaper = Convert.ToString(row["wallpaper"]);
            Floor = Convert.ToString(row["floor"]);
            Landscape = Convert.ToString(row["landscape"]);
            FloorThickness = Convert.ToInt32(row["floorthick"]);
            WallThickness = Convert.ToInt32(row["wallthick"]);
            WhoCanMute = Convert.ToInt32(row["mute_settings"]);
            WhoCanKick = Convert.ToInt32(row["kick_settings"]);
            WhoCanBan = Convert.ToInt32(row["ban_settings"]);
            ChatMode = Convert.ToInt32(row["chat_mode"]);
            ChatSpeed = Convert.ToInt32(row["chat_speed"]);
            ChatSize = Convert.ToInt32(row["chat_size"]);
            TradeSettings = Convert.ToInt32(row["trade_settings"]);

            Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(Convert.ToInt32(row["group_id"]));

            foreach (var tag in row["tags"].ToString().Split(','))
                Tags.Add(tag);

            _mModel = OblivionServer.GetGame().GetRoomManager().GetModel(ModelName);

            WiredScoreBordDay = new Dictionary<int, KeyValuePair<int, string>>();
            WiredScoreBordWeek = new Dictionary<int, KeyValuePair<int, string>>();
            WiredScoreBordMonth = new Dictionary<int, KeyValuePair<int, string>>();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                var SuperCheck = new List<bool>
                {
                    false,
                    false,
                    false
                };

                var now = DateTime.Now;
                var getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
                var getmonthtoday = Convert.ToInt32(now.ToString("MM"));
                var getweektoday = CultureInfo.GetCultureInfo("Nl-nl")
                    .Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                WiredScoreFirstBordInformation = new List<int>
                {
                    getdaytoday,
                    getmonthtoday,
                    getweektoday
                };

                dbClient.SetQuery("SELECT * FROM wired_scorebord WHERE roomid = @id ORDER BY `punten` DESC ");
                dbClient.AddParameter("id", Id);
                foreach (DataRow nrow in dbClient.GetTable().Rows)
                {
                    var userid = Convert.ToInt32(nrow["userid"]);
                    var username = Convert.ToString(nrow["username"]);
                    var Punten = Convert.ToInt32(nrow["punten"]);
                    var soort = Convert.ToString(nrow["soort"]);
                    var timestamp = Convert.ToInt32(nrow["timestamp"]);
                    if (soort == "day" && !WiredScoreBordDay.ContainsKey(userid) && !SuperCheck[0])
                    {
                        SuperCheck[0] = timestamp > getdaytoday;
                        if (!SuperCheck[0])
                            WiredScoreBordDay.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                    if (soort == "month" && !WiredScoreBordMonth.ContainsKey(userid) && !SuperCheck[1])
                    {
                        SuperCheck[1] = timestamp > getmonthtoday;
                        WiredScoreBordMonth.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                    if (soort == "week" && !WiredScoreBordWeek.ContainsKey(userid) && !SuperCheck[2])
                    {
                        SuperCheck[2] = timestamp > getweektoday;
                        WiredScoreBordWeek.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                }
                if (SuperCheck[0])
                {
                    dbClient.RunFastQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", Id,
                        "' AND `soort`='day'"));
                    WiredScoreBordDay.Clear();
                }
                if (SuperCheck[1])
                {
                    dbClient.RunFastQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", Id,
                        "' AND `soort`='month'"));
                    WiredScoreBordMonth.Clear();
                }
                if (SuperCheck[2])
                {
                    dbClient.RunFastQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", Id,
                        "' AND `soort`='week'"));
                    WiredScoreBordDay.Clear();
                }

                dbClient.SetQuery(
                    $"SELECT command_name FROM room_blockcmd WHERE room_id = '{Id}'");
                var table = dbClient.GetTable();
                foreach (DataRow data in table.Rows)
                    BlockedCommands.Add(data["command_name"].ToString());
            }
        }

        public void EndPromotion()
        {
            if (!HasActivePromotion)
                return;

            Promotion = null;
        }
    }
}