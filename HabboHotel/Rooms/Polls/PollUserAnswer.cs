#region

using System;
using System.Data;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls
{
    public class PollUserAnswer
    {
        public string AnswerData;
        public int Main;
        public int PollId;
        public int QuestionId;


        public PollUserAnswer(int main, int questid, int pollid, string answerdata)
        {
            Main = main;
            QuestionId = questid;
            AnswerData = answerdata;
            PollId = pollid;
        }

        public PollUserAnswer(int main, int pollid, DataRow Row)
        {
            Main = main;
            PollId = pollid;

            QuestionId = Convert.ToInt32(Row["question_id"]);
            AnswerData = Row["answer_data"].ToString();
        }
    }
}