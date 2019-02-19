#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Rooms.Polls
{
    public class Poll
    {
        public string CodeName;
        public string Desc;
        public string Greeting;
        public int Id;
        public int Limit;

        public List<PollQuestion> Questions;
        public int RoomId;

        public bool SomeBool;
        public string Title;

        public Poll(DataRow Row)
        {
            Id = Convert.ToInt32(Row["id"]);
            RoomId = Convert.ToInt32(Row["room_id"]);
            Limit = Convert.ToInt32(Row["limit"]);

            Title = Row["title"].ToString();
            Desc = Row["description"].ToString();
            Greeting = Row["greetings_text"].ToString();
            CodeName = Row["code_name"].ToString();

            Questions = PollQuestion.Generate(this);
            SomeBool = true;
        }

        public int QuestionsCountAll
        {
            get
            {
                var i = 0;
                foreach (var q in Questions)
                {
                    i++;
                    i += q.SubQuestions.Count;
                }

                return i;
            }
        }

        public int QuestionsCount => Questions.Count;

        public PollQuestion AddQuestion(int id, int order, string question, PollQuestionType type)
        {
            var q = new PollQuestion(id, order, question, type);
            Questions.Add(q);
            return q;
        }

        public PollQuestion GetQuestion(int id)
        {
            foreach (var quest in Questions)
            {
                if (quest.Id == id)
                    return quest;

                foreach (var subquest in quest.SubQuestions.Where(subquest => subquest.Id == id))
                    return subquest;
            }

            return null;
        }

        /*public int GetQuestionCountForOcasion(UserPollData User)
        {
            var i = 0;
            List<PollQuestion> ListedQuestions = new List<PollQuestion>();
            foreach (var answer in User.Answers)
            {
                
            }
        }*/

        /*public Dictionary<int, PollQuestion> GetQuestionSteps()
        {
            var i = new Dictionary<int, PollQuestion>();
            for (var n = 1; n < Questions.Count; n++)
            {
                i.Add(n, Questions[n - 1]);
            }

            return i;
        }*/

        public bool UserFinishedPoll(UserPollData User)
        {
            var i = 0;
            foreach (var q in Questions)
            {
                if (User.Answers.Any(c => c.QuestionId == q.Id))
                    i++;

                if (q.SubQuestions.Count > 0)
                    if (User.Answers.All(c => q.SubQuestions.FirstOrDefault(d => d.Id == c.QuestionId) == null))
                        i--;
            }

            return i >= Questions.Count;
        }
    }
}