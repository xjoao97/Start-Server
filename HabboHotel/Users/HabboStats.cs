using System.Collections.Generic;
using System.Linq;

namespace Oblivion.HabboHotel.Users
{
    public class HabboStats
    {
        public HabboStats(int roomVisits, double onlineTime, int Respect, int respectGiven, int giftsGiven,
            int giftsReceived, int dailyRespectPoints, int dailyPetRespectPoints, int achievementPoints, int questID,
            int questProgress, int groupID, string RespectsTimestamp, int ForumPosts, string openedGifts)
        {
            RoomVisits = roomVisits;
            OnlineTime = onlineTime;
            this.Respect = Respect;
            RespectGiven = respectGiven;
            GiftsGiven = giftsGiven;
            GiftsReceived = giftsReceived;
            DailyRespectPoints = dailyRespectPoints;
            DailyPetRespectPoints = dailyPetRespectPoints;
            AchievementPoints = achievementPoints;
            QuestID = questID;
            QuestProgress = questProgress;
            FavouriteGroupId = groupID;
            this.RespectsTimestamp = RespectsTimestamp;
            this.ForumPosts = ForumPosts;
            this.openedGifts = new List<int>();
            foreach (var subStr in openedGifts.Split(','))
            {
                int openedDay;
                if (int.TryParse(subStr, out openedDay))
                    this.openedGifts.Add(openedDay);
            }
        }

        public int RoomVisits { get; set; }
        public double OnlineTime { get; set; }
        public int Respect { get; set; }
        public int RespectGiven { get; set; }
        public int GiftsGiven { get; set; }
        public int GiftsReceived { get; set; }
        public int DailyRespectPoints { get; set; }
        public int DailyPetRespectPoints { get; set; }
        public int AchievementPoints { get; set; }
        public int QuestID { get; set; }
        public int QuestProgress { get; set; }
        public int FavouriteGroupId { get; set; }
        public string RespectsTimestamp { get; set; }
        public int ForumPosts { get; set; }
        public List<int> openedGifts { get; }

        public void addOpenedGift(int eventDate, int habboId)
        {
            if (openedGifts.Contains(eventDate))
                return;

            openedGifts.Add(eventDate);
            var giftData = openedGifts.Select(giftDay => giftDay.ToString()).ToArray();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_stats` SET `calendar_gifts` = @giftData WHERE `id` = @habboId LIMIT 1");
                dbClient.AddParameter("giftData", string.Join(",", giftData));
                dbClient.AddParameter("habboId", habboId);
                dbClient.RunQuery();
            }
        }
    }
}