#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Polls;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.Polls;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Polls
{
    internal class AcceptPollQuestionsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PollId = Packet.PopInt();
            Poll Poll;
            if (!RoomPollManager.TryGetPoll(PollId, out Poll))
            {
                Session.SendMessage(new PollErrorAlertComposer());
                return;
            }

            RoomPollManager.StartUserAnsweringPoll(Session.GetHabbo().Id, PollId);
            Session.SendMessage(new PollQuestionsComposer(Poll));
        }
    }
}