using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Database.Interfaces;

namespace Oblivion.HabboHotel.Catalog.FurniMatic
{
    public class FurniMaticRewardsManager
    {
        private List<FurniMaticRewards> Rewards;

        public List<FurniMaticRewards> GetRewards() => Rewards;

        public List<FurniMaticRewards> GetRewardsByLevel(int level)
            => Rewards.Where(furni => furni.Level == level).ToList();

        public FurniMaticRewards GetRandomReward()
        {
            var level = 1;
            if (new Random().Next(1, 100) == 100) level = 5;
            else if (new Random().Next(1, 50) == 50) level = 4;
            else if (new Random().Next(1, 20) == 20) level = 3;
            else if (new Random().Next(1, 5) == 5) level = 2;
            var possibleRewards = GetRewardsByLevel(level);
            if (possibleRewards.Count > 0)
                return possibleRewards[new Random().Next(0, possibleRewards.Count - 1)];
            var rand = new Random().Next(possibleRewards.Max().BaseId, possibleRewards.Min().BaseId);
            return new FurniMaticRewards(0, rand, 0);
        }

        public void Init(IQueryAdapter dbClient)
        {
            Rewards = new List<FurniMaticRewards>();
            dbClient.SetQuery("SELECT display_id, item_id, reward_level FROM ecotron_rewards");
            var table = dbClient.GetTable();
            if (table == null) return;
            foreach (DataRow row in table.Rows)
                Rewards.Add(new FurniMaticRewards(Convert.ToInt32(row["display_id"]), Convert.ToInt32(row["item_id"]),
                    Convert.ToInt32(row["reward_level"])));
        }
    }
}