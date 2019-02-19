namespace Oblivion.HabboHotel.Moderation
{
    public class ModerationBan
    {
        public double Expire;
        public string Reason;
        public ModerationBanType Type;
        public string Value;

        public ModerationBan(ModerationBanType Type, string Value, string Reason, double Expire)
        {
            this.Type = Type;
            this.Value = Value;
            this.Reason = Reason;
            this.Expire = Expire;
        }

        public bool Expired => OblivionServer.GetUnixTimestamp() >= Expire;
    }
}