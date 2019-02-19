#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Polls.Quick;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Polls;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Polls
{
    internal class AnswerPollQuestionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PollId = Packet.PopInt();
            var QuestId = Packet.PopInt();

            var AnswerLenght = Packet.PopInt();
            var Answers = new List<string>();
            while (AnswerLenght > 0)
            {
                Answers.Add(Packet.PopString());
                AnswerLenght--;
            }


            if (PollId == -2)
            {
                //QuickPoll
                var Room = Session.GetHabbo().CurrentRoom;

                if (Room?.QuickPoll == null)
                    return;

                var Voted = Answers.FirstOrDefault() == "1";
                Room.QuickPoll.AddVote(Session.GetHabbo().Id, Voted);


                var user = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
                if (user.IsBot) return;
                Room.SendMessage(new ConcludePollComposer(Room.QuickPoll, Session.GetHabbo().Id, Voted ? 1 : 0));
                Session.GetHabbo().AnswredPoll = true;
                return;
            }


            var Poll = RoomPollManager.GetPoll(PollId);
            if (Poll == null)
                return;

            var userAnsweringPoll = RoomPollManager.GetUserAnsweringPoll(Session.GetHabbo().Id, PollId);

            // var quest = Poll.GetQuestion(


            userAnsweringPoll?.Answer(QuestId, string.Join("\t", Answers.ToArray()));

            if (userAnsweringPoll?.CompletedAllQuestions == true)
                RoomPollManager.FinishuserAnsweringPoll(userAnsweringPoll.UserId);
        }
    }
}