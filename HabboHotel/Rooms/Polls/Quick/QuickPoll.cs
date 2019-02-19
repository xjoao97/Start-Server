#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Polls.Quick;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls.Quick
{
    public class QuickPoll
    {
        public int Id;
        public int TimeLen;

        public QuickPoll(int id, Room room, string Title, int length)
        {
            this.Title = Title;
            Id = id;
            TimeLen = length;
            Room = room;
            UsersPolls = new List<UserQuickPoll>();
        }

        public string Title { get; set; }
        public List<UserQuickPoll> UsersPolls { get; set; }
        public Room Room { get; set; }

        public int StartTime { get; set; }

        public int ReamigTime => Math.Max(0, StartTime - (int) OblivionServer.GetUnixTimestamp());

        public List<UserQuickPoll> PVotes => UsersPolls.Where(c => c.Yes).ToList();

        public List<UserQuickPoll> NVotes => UsersPolls.Where(c => c.No).ToList();

        public void Start()
        {
            StartTime = (int) OblivionServer.GetUnixTimestamp() + TimeLen;
            PerformStart();
        }

        public void PerformStart() => Room.SendMessage(new MatchingPollComposer(this));

        public void PerformStartTo(GameClient Session)
        {
            if (ReamigTime == 0 || Voted(Session.GetHabbo().Id))
                return;

            Session.SendMessage(new MatchingPollComposer(this));
        }

        public void Stop()
        {
            var users = Room.GetRoomUserManager().GetRoomUsers();
            TimeLen = 3;
            for (var i = 0; i <= 10; i++)
            {
                foreach (var user in users.ToList().Select(roomuser => OblivionServer.GetHabboById(roomuser.UserId)).Where(user => user.AnswredPoll))
                {
                    user.GetClient().SendMessage(new ConcludePollComposer(Room.QuickPoll, user.Id, 1));
                }

            }
            foreach (var roomuser in users.ToList())
                OblivionServer.GetHabboById(roomuser.UserId).AnswredPoll = false;
        }

        public bool Voted(int Userid) => UsersPolls.Any(c => c.UserId == Userid);

        public void AddVote(int UserId, bool VoteYes) => UsersPolls.Add(new UserQuickPoll(UserId, VoteYes));
    }
}