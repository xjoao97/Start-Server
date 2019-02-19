#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls
{
    public class PollQuestion
    {
        public List<PollAnswerItem> Answers;
        public int Id;

        public int OrderNum;

        public PollQuestion ParentQuestion;
        public int ParentQuestionId;
        public Poll Poll;

        public string Question;
        public List<PollQuestion> SubQuestions;

        public PollQuestionType Type;

        public PollQuestion(DataRow Row, Poll Main)
        {
            Poll = Main;
            Id = Convert.ToInt32(Row["id"]);
            OrderNum = Convert.ToInt32(Row["order_num"]);
            ParentQuestionId = Convert.ToInt32(Row["parent_id"]);
            Question = Row["question"].ToString();
            Type = (PollQuestionType) Convert.ToInt32(Row["type"]);

            Answers = PollAnswerItem.Generate(this);
            SubQuestions = new List<PollQuestion>();
        }


        //Normal Quest
        public PollQuestion(int id, Poll poll, PollQuestion parentquest, int order, string question,
            List<PollAnswerItem> answers, PollQuestionType type, List<PollQuestion> subquests)
        {
            Id = id;
            Poll = poll;
            ParentQuestion = parentquest;
            OrderNum = order;
            Question = question;
            Answers = answers;
            Type = type;
            SubQuestions = subquests;
        }

        //Question Parent
        public PollQuestion(int id, Poll poll, int order, string question, List<PollAnswerItem> answers,
            PollQuestionType type)
        {
            Id = id;
            Poll = poll;
            ParentQuestion = null;
            OrderNum = order;
            Question = question;
            Answers = answers;
            Type = type;
            SubQuestions = null;
        }

        public PollQuestion(int id, int order, string question, PollQuestionType type)
        {
            Id = id;
            OrderNum = order;
            Question = question;
            Answers = new List<PollAnswerItem>();
            SubQuestions = new List<PollQuestion>();
            ParentQuestion = null;
            Type = type;
        }

        private PollQuestion(int id, int order, PollQuestion parentquest, string question, PollQuestionType type)
        {
            Id = id;
            OrderNum = order;
            Question = question;
            Answers = new List<PollAnswerItem>();
            SubQuestions = new List<PollQuestion>();
            ParentQuestion = parentquest;
            Type = type;
        }

        public static List<PollQuestion> Generate(Poll Poll)
        {
            DataTable Table1;
            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.SetQuery("SELECT * FROM server_polls_questions WHERE poll_id = '" + Poll.Id + "'");
                Table1 = adap.getTable();
            }

            var list = new List<PollQuestion>();
            var children = new List<PollQuestion>();

            foreach (var q in from DataRow Row in Table1.Rows select new PollQuestion(Row, Poll))
                if (q.ParentQuestionId > 0)
                    children.Add(q);
                else
                    list.Add(q);

            foreach (var item in list)
            foreach (var child in children.Where(child => child.ParentQuestionId == item.Id))
            {
                child.ParentQuestion = item;
                item.SubQuestions.Add(child);
            }

            return list;
        }

        public PollAnswerItem AddAnswer(int id, string answertext, string answerkey, int answerkeyint, int targetsub = 0)
        {
            var i = new PollAnswerItem(id, answertext, this, targetsub);
            Answers.Add(i);
            return i;
        }

        public PollQuestion AddSubQuestion(int id, int order, string Question, PollQuestionType type)
        {
            var q = new PollQuestion(id, order, this, Question, type);
            SubQuestions.Add(q);
            return q;
        }

        public PollQuestion GetSubQuestion(int id) => SubQuestions.FirstOrDefault(c => c.Id == id);
    }

    public enum PollQuestionType
    {
        RADIO = 1,
        CHECKBOX = 2,
        TEXTBOX = 3,
        TYPE_4 = 4
    }
}