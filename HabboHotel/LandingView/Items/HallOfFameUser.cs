namespace Oblivion.HabboHotel.LandingView.Items
{
    public class HallOfFameUser
    {
        public int Score;
        public int UserId;

        public HallOfFameUser(int userid, int score)
        {
            UserId = userid;
            Score = score;
        }
    }
}