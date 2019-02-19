#region



#endregion

namespace Oblivion.HabboHotel.Rooms.Polls.Quick
{
    public class UserQuickPoll
    {
        public int UserId;
        public bool Yes;

        public UserQuickPoll(int uid, bool yes)
        {
            UserId = uid;
            Yes = yes;
        }

        public bool No => !Yes;
    }
}