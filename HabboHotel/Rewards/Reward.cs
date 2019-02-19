#region

using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Rewards
{
    public class Reward
    {
        public Reward(double Start, double End, string Type, string RewardData, string Message)
        {
            RewardStart = Start;
            RewardEnd = End;
            this.Type = RewardTypeUtility.GetType(Type);
            this.RewardData = RewardData;
            this.Message = Message;
        }

        public double RewardStart { get; set; }
        public double RewardEnd { get; set; }
        public RewardType Type { get; set; }
        public string RewardData { get; set; }
        public string Message { get; set; }

        public bool isActive()
        {
            var Now = UnixTimestamp.GetNow();
            return Now >= RewardStart && Now <= RewardEnd;
        }
    }
}