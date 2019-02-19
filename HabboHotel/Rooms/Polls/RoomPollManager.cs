#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls
{
    public static class RoomPollManager
    {
        public static List<Poll> Polls;
        public static List<UserPollData> UsersCachedPolls;
        public static Dictionary<int, UserPollData> UsersAnsweringPolls;
        public static List<UserPollDecline> UsersDeclinedPolls;

        public static int PollDeclineTimeout = 1800;


        public static void Dispose(Habbo habbo)
        {
            var client = habbo?.GetClient();

            if (client == null)
                return;

            var userid = client.GetHabbo().Id;

            if (UsersAnsweringPolls.ContainsKey(userid))
                UsersAnsweringPolls.Remove(userid);
        }

        #region Polls

        public static bool TryGetPoll(int Id, out Poll poll)
        {
            poll = GetPoll(Id);

            return poll != null;
        }

        public static void DeclinePoll(int userid, int pollid)
            => UsersDeclinedPolls.Add(new UserPollDecline(userid, pollid, (int) OblivionServer.GetUnixTimestamp()));


        public static Poll GetPoll(int id) => Polls.FirstOrDefault(c => c.Id == id);

        #endregion

        #region Users Answered Polls

        #endregion

        #region Users Answering Polls

        public static void StartUserAnsweringPoll(int userid, int pollid)
        {
            if (GetPoll(pollid) == null)
                return;
            RemoveUserAnsweringPoll(userid);
            UsersAnsweringPolls.Add(userid, new UserPollData(userid, pollid));
        }

        public static UserPollData GetUserAnsweringPoll(int userid, int pollid)
        {
            if (UsersAnsweringPolls.ContainsKey(userid))
            {
                var usera = UsersAnsweringPolls[userid];
                if (usera.PollId != pollid)
                {
                    UsersAnsweringPolls.Remove(userid);
                    return null;
                }

                return usera;
            }
            return null;
        }

        public static void RemoveUserAnsweringPoll(int userid)
        {
            if (UsersAnsweringPolls.ContainsKey(userid))
                UsersAnsweringPolls.Remove(userid);
        }

        public static void FinishuserAnsweringPoll(int userid)
        {
            if (!UsersAnsweringPolls.ContainsKey(userid))
                return;

            var userapoll = UsersAnsweringPolls[userid];

            userapoll.Finish();
            UsersAnsweringPolls.Remove(userid);
            UsersCachedPolls.Add(userapoll);
        }

        #endregion;
    }
}