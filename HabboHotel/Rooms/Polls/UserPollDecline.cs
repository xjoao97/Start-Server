namespace Oblivion.HabboHotel.Rooms.Polls
{
    public class UserPollDecline
    {
        public int PollId;
        public int Timestamp;
        public int UserId;

        public UserPollDecline(int userid, int poll, int time)
        {
            UserId = userid;
            PollId = poll;
            Timestamp = time;
        }
    }
}