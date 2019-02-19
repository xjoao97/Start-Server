#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Inventory.Achievements
{
    internal class AchievementsComposer : ServerPacket
    {
        public AchievementsComposer(GameClient Session, List<Achievement> Achievements)
            : base(ServerPacketHeader.AchievementsMessageComposer)
        {
            WriteInteger(Achievements.Count);
            foreach (var Achievement in Achievements)
            {
                var UserData = Session.GetHabbo().GetAchievementData(Achievement.GroupName);
                var TargetLevel = UserData?.Level + 1 ?? 1;
                var TotalLevels = Achievement.Levels.Count;

                TargetLevel = TargetLevel > TotalLevels ? TotalLevels : TargetLevel;

                var TargetLevelData = Achievement.Levels[TargetLevel];
                WriteInteger(Achievement.Id); // Unknown (ID?)
                WriteInteger(TargetLevel); // Target level
                WriteString(Achievement.GroupName + TargetLevel); // Target name/desc/badge

                WriteInteger(1);
                WriteInteger(TargetLevelData.Requirement); // Progress req/target          
                WriteInteger(TargetLevelData.RewardPixels);

                WriteInteger(0); // Type of reward
                WriteInteger(UserData?.Progress ?? 0); // Current progress

                WriteBoolean(UserData != null && UserData.Level >= TotalLevels); // Set 100% completed(??)
                WriteString(Achievement.Category); // Category
                WriteString(string.Empty);
                WriteInteger(TotalLevels); // Total amount of levels 
                WriteInteger(0);
            }
            WriteString("");
        }
    }
}