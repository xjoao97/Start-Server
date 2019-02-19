#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls
{
    public class UserPollData
    {
        public List<PollUserAnswer> Answers;
        public bool Finished;
        public int Id;
        public int PollId;
        public int Timestamp;
        public int UserId;

        public UserPollData(int userid, int pollid)
        {
            UserId = userid;
            PollId = pollid;
            Timestamp = (int) OblivionServer.GetUnixTimestamp();
            Answers = new List<PollUserAnswer>();
        }

        public UserPollData(DataRow MainRow, DataTable Table)
        {
            Finished = true;
            Answers = new List<PollUserAnswer>();
            Id = Convert.ToInt32(MainRow["id"]);
            UserId = Convert.ToInt32(MainRow["user_id"]);
            PollId = Convert.ToInt32(MainRow["poll_id"]);
            Timestamp = Convert.ToInt32(MainRow["timestamp"]);

            foreach (
                var Row in
                from DataRow Row in Table.Rows
                let pollid = Convert.ToInt32(Row["main_id"])
                where pollid == PollId
                select Row)
                Answers.Add(new PollUserAnswer(Id, PollId, Row));
        }

        public bool CompletedAllQuestions
        {
            get
            {
                var poll = GetPoll();
                return poll != null && poll.UserFinishedPoll(this);
            }
        }

        public Poll GetPoll() => RoomPollManager.GetPoll(PollId);

        public void Answer(int questId, string answer)
        {
            var poll = GetPoll();

            var question = poll?.GetQuestion(questId);
            if (question == null)
                return;

            Answers.Add(new PollUserAnswer(Id, questId, PollId, answer));
        }

        public void Finish()
        {
            if (Finished)
                return;
            int Insert;
            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.SetQuery(string.Concat("INSERT INTO server_polls_users (user_id, poll_id, timestamp) VALUES ('",
                    UserId, "', '", PollId, "', '", Timestamp, "')"));
                Insert = (int) adap.InsertQuery();
            }

            Id = Insert;

            var str = new StringBuilder();
            str.Append("INSERT INTO server_polls_users_answers (main_id, question_id, answer_data) VALUES ");
            foreach (var answ in Answers)
            {
                answ.Main = Id;
                str.AppendLine("(");

                str.Append(string.Concat("'", Id, "', '", answ.QuestionId, "', '", answ.AnswerData, "'"));

                str.Append(")");
                str.Append(answ == Answers.Last() ? ";" : ",");
            }

            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.RunQuery(str.ToString());
            }

            Finished = true;
        }
    }
}