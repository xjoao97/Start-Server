#region

using Oblivion.HabboHotel.Rooms.Polls;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Polls
{
    internal class PollQuestionsComposer : ServerPacket
    {
        public PollQuestionsComposer(Poll Poll)
            : base(ServerPacketHeader.PollQuestionsMessageComposer)
        {
            /*goto nrom;
            base.WriteInteger(1);
            base.WriteString("Title");//Title
            base.WriteString("Vlw"); //Greeting

            base.WriteInteger(1); //while quests
            
            base.WriteInteger(1);//quest id
            base.WriteInteger(1);// quest order
            base.WriteInteger(1); //quest type
            base.WriteString("Eu Sou gay?");

            base.WriteInteger(0); //Unknown
            base.WriteInteger(0); //Unknown

            base.WriteInteger(2);// Answers amount

            base.WriteString("1");
            base.WriteString("Sim");
            base.WriteInteger(3);

            base.WriteString("2");
            base.WriteString("Não");
            base.WriteInteger(3);

            base.WriteInteger(1);

            base.WriteInteger(3);
            base.WriteInteger(3);
            base.WriteInteger(3);
            base.WriteString("Serio?");

            base.WriteInteger(3);
            base.WriteInteger(0);

            base.WriteInteger(0);

            base.WriteBoolean(true);




            return;
            nrom:*/
            WriteInteger(Poll.Id);
            WriteString(Poll.Title);
            WriteString(Poll.Greeting);

            WriteInteger(Poll.Questions.Count);

            foreach (var Quest in Poll.Questions)
            {
                WriteInteger(Quest.Id);
                WriteInteger(Quest.Id);
                WriteInteger((int) Quest.Type);
                WriteString(Quest.Question);

                WriteInteger(Quest.Id);
                WriteInteger(-1);

                WriteInteger((int) Quest.Type < 3 ? Quest.Answers.Count : 0);
                if ((int) Quest.Type < 3)
                    foreach (var Answer in Quest.Answers)
                    {
                        WriteString(Answer.Id.ToString());
                        WriteString(Answer.AnswerText);
                        WriteInteger(Answer.TargetSubQuestionId); //Maybe Parent  Question ID or Actual Question ID
                    }

                WriteInteger(Quest.SubQuestions.Count);
                foreach (var SubQuest in Quest.SubQuestions)
                {
                    WriteInteger(SubQuest.Id);
                    WriteInteger(SubQuest.Id);
                    WriteInteger((int) SubQuest.Type);
                    WriteString(SubQuest.Question);

                    WriteInteger(SubQuest.Id);
                    WriteInteger(-1);

                    WriteInteger((int) SubQuest.Type < 3 ? SubQuest.Answers.Count : 0);
                    if ((int) SubQuest.Type < 3)
                        foreach (var Answer in SubQuest.Answers)
                        {
                            WriteString(Answer.Id.ToString());
                            WriteString(Answer.AnswerText);
                            WriteInteger(Answer.TargetSubQuestionId); //Maybe Parent  Question ID or Actual Question ID
                        }
                }
            }

            WriteBoolean(Poll.SomeBool);
        }

        public void WriteQuestion(PollQuestion Quest)
        {
            WriteInteger(Quest.Id);
            WriteInteger(Quest.Id);
            WriteInteger((int) Quest.Type);
            WriteString(Quest.Question);

            WriteInteger(0);
            WriteInteger(0);

            WriteInteger((int) Quest.Type < 3 ? Quest.Answers.Count : 0);
            if ((int) Quest.Type < 3)
                foreach (var Answer in Quest.Answers)
                {
                    WriteString(Answer.Id.ToString());
                    WriteString(Answer.AnswerText);
                    WriteInteger(Answer.TargetSubQuestionId); //Maybe Parent  Question ID or Actual Question ID
                }
        }
    }
}