#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls
{
    public class PollAnswerItem
    {
        public string AnswerText;
        public int Id;
        public bool IsCorrectAnswer;

        public PollQuestion ParentQuestion;
        public int TargetSubQuestionId;

        public PollAnswerItem(DataRow Row, PollQuestion Main)
        {
            ParentQuestion = Main;

            Id = Convert.ToInt32(Row["id"]);
            AnswerText = Row["value"].ToString();
            IsCorrectAnswer = Convert.ToInt32(Row["is_correct"]) == 1;
            TargetSubQuestionId = Convert.ToInt32(Row["target_subquestion"]);
        }


        public PollAnswerItem(int id, string answertext, PollQuestion parentquestion, int targetsubqid)
        {
            Id = id;
            AnswerText = answertext;
            ParentQuestion = parentquestion;
            TargetSubQuestionId = targetsubqid;
        }


        public static List<PollAnswerItem> Generate(PollQuestion Quest)
        {
            DataTable Table1;
            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.SetQuery("SELECT * FROM server_polls_questions_answers WHERE question_id = '" + Quest.Id + "'");

                Table1 = adap.GetTable();
            }

            return (from DataRow Row in Table1.Rows select new PollAnswerItem(Row, Quest)).ToList();
        }
    }
}