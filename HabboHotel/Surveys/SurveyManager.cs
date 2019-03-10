#region

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Surveys
{
    internal class SurveyManager
    {
        private readonly ConcurrentDictionary<int, Question> _questions;

        public SurveyManager()
        {
            _questions = new ConcurrentDictionary<int, Question>();

            Init();
        }

        public void Init()
        {
            DataTable Table;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `questions`");
                Table = dbClient.GetTable();
            }

            if (Table != null)
                foreach (
                    var Row in
                    Table.Rows.Cast<DataRow>().Where(Row => !_questions.ContainsKey(Convert.ToInt32(Row["id"]))))
                    _questions.TryAdd(Convert.ToInt32(Row["id"]), new Question());
        }

        public bool TryGetQuestion(int QuestionId, out Question Question)
            => _questions.TryGetValue(QuestionId, out Question);
    }
}