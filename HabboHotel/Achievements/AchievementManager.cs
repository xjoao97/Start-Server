#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using log4net;
using Oblivion.Communication.Packets.Outgoing.Inventory.Achievements;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.HabboHotel.Achievements
{
    public class AchievementManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Achievements.AchievementManager");

        public HybridDictionary _achievements;

        public AchievementManager()
        {
            _achievements = new HybridDictionary();
            LoadAchievements();

            log.Info("Conquistas -> CARREGADO");
        }

        public void LoadAchievements() => AchievementLevelFactory.GetAchievementLevels(out _achievements);

        internal void ProgressLoginAchievements(GameClient session)
        {
            if (session.GetHabbo() == null)
                return;

            if (session.GetHabbo().Achievements.ContainsKey("ACH_Login"))
            {
                var daysBtwLastLogin = OblivionServer.GetUnixTimestamp() - session.GetHabbo().LastOnline;

                if (daysBtwLastLogin >= 51840 && daysBtwLastLogin <= 112320)
                    ProgressAchievement(session, "ACH_Login", 1, true);

                return;
            }

            ProgressAchievement(session, "ACH_Login", 1, true);
        }

        internal void ProgressRegistrationAchievements(GameClient session)
        {
            if (session.GetHabbo() == null)
                return;

            if (session.GetHabbo().Achievements.ContainsKey("ACH_RegistrationDuration"))
            {
                var regAch = session.GetHabbo().GetAchievementData("ACH_RegistrationDuration");

                if (regAch.Level == 5)
                    return;

                var sinceMember = OblivionServer.GetUnixTimestamp() - session.GetHabbo().AccountCreated;

                var daysSinceMember = Convert.ToUInt32(Math.Round(sinceMember / 86400));

                if (daysSinceMember == regAch.Progress)
                    return;

                var days = daysSinceMember - regAch.Progress;

                if (days < 1)
                    return;

                ProgressAchievement(session, "ACH_RegistrationDuration", (int) days);

                return;
            }

            ProgressAchievement(session, "ACH_RegistrationDuration", 1, true);
        }


        public bool ProgressAchievement(GameClient Session, string AchievementGroup, int ProgressAmount,
            bool FromZero = false)
        {
            if (!_achievements.Contains(AchievementGroup) || Session == null)
                return false;

            var AchievementData = (Achievement) _achievements[AchievementGroup];

            var UserData = Session.GetHabbo().GetAchievementData(AchievementGroup);
            if (UserData == null)
            {
                UserData = new UserAchievement(AchievementGroup, 0, 0);
                Session.GetHabbo().Achievements.TryAdd(AchievementGroup, UserData);
            }

            var TotalLevels = AchievementData.Levels.Count;

            if (UserData.Level == TotalLevels)
                return false; // done, no more.

            var TargetLevel = UserData.Level + 1;

            if (TargetLevel > TotalLevels)
                TargetLevel = TotalLevels;

            var TargetLevelData = AchievementData.Levels[TargetLevel];
            int NewProgress;
            if (FromZero)
                NewProgress = ProgressAmount;
            else
                NewProgress = UserData.Progress + ProgressAmount;

            var NewLevel = UserData.Level;
            var NewTarget = NewLevel + 1;

            if (NewTarget > TotalLevels)
                NewTarget = TotalLevels;

            if (NewProgress >= TargetLevelData.Requirement)
            {
                NewLevel++;
                NewTarget++;

                //    var ProgressRemainder = NewProgress - TargetLevelData.Requirement;

                NewProgress = 0;

                if (TargetLevel == 1)
                {
                    Session.GetHabbo().GetBadgeComponent().GiveBadge(AchievementGroup + TargetLevel, true, Session);
                }
                else
                {
                    Session.GetHabbo()
                        .GetBadgeComponent()
                        .RemoveBadge(Convert.ToString(AchievementGroup + (TargetLevel - 1)));
                    Session.GetHabbo().GetBadgeComponent().GiveBadge(AchievementGroup + TargetLevel, true, Session);
                }

                if (NewTarget > TotalLevels)
                    NewTarget = TotalLevels;


                Session.SendMessage(new AchievementUnlockedComposer(AchievementData, TargetLevel,
                    TargetLevelData.RewardPoints, TargetLevelData.RewardPixels));
//                Session.GetHabbo()
//                    .GetMessenger()
//                    .BroadcastAchievement(Session.GetHabbo().Id, MessengerEventTypes.ACHIEVEMENT_UNLOCKED,
//                        AchievementGroup + TargetLevel);

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" + Session.GetHabbo().Id +
                                      "', @group, '" + NewLevel + "', '" + NewProgress + "')");
                    dbClient.AddParameter("group", AchievementGroup);
                    dbClient.RunQuery();
                }

                UserData.Level = NewLevel;
                UserData.Progress = NewProgress;

                Session.GetHabbo().Duckets += TargetLevelData.RewardPixels;
                Session.GetHabbo().GetStats().AchievementPoints += TargetLevelData.RewardPoints;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets,
                    TargetLevelData.RewardPixels));
                Session.SendMessage(new AchievementScoreComposer(Session.GetHabbo().GetStats().AchievementPoints));

                var NewLevelData = AchievementData.Levels[NewTarget];
                Session.SendMessage(new AchievementProgressedComposer(AchievementData, NewTarget, NewLevelData,
                    TotalLevels, Session.GetHabbo().GetAchievementData(AchievementGroup)));

                return true;
            }
            UserData.Level = NewLevel;
            UserData.Progress = NewProgress;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" + Session.GetHabbo().Id + "', @group, '" +
                                  NewLevel + "', '" + NewProgress + "')");
                dbClient.AddParameter("group", AchievementGroup);
                dbClient.RunQuery();
            }

            Session.SendMessage(new AchievementProgressedComposer(AchievementData, TargetLevel, TargetLevelData,
                TotalLevels, Session.GetHabbo().GetAchievementData(AchievementGroup)));
            return false;
        }

        public List<Achievement> GetGameAchievements(int GameId)
            =>
                _achievements.Values.Cast<Achievement>().ToList()
                    .Where(Achievement => Achievement.Category == "games" && Achievement.GameId == GameId)
                    .ToList();
    }
}