#region

using Oblivion.HabboHotel.Rooms.Polls.Quick;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Polls.Quick
{
    internal class ConcludePollComposer : ServerPacket
    {
        public ConcludePollComposer(QuickPoll Poll, int UserId, int VoteType)
            : base(ServerPacketHeader.MatchingPollFinishMessageComposer)
        {
            WriteInteger(UserId); //User id
            WriteString(VoteType.ToString()); //1 to right / otherwise wrong
            WriteInteger(2); //len

            WriteString("1");
            WriteInteger(Poll.PVotes.Count);

            WriteString("0");
            WriteInteger(Poll.NVotes.Count);
        }
    }
}