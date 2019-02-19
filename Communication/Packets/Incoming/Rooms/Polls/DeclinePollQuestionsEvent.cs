#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms.Polls;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Polls
{
    internal class DeclinePollQuestionsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PollId = Packet.PopInt();
            RoomPollManager.DeclinePoll(Session.GetHabbo().Id, PollId);
        }
    }
}