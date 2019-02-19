#region

using System.Web;
using Oblivion.HabboHotel.Rooms.Polls.Quick;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Polls.Quick
{
    internal class MatchingPollComposer : ServerPacket
    {
        public MatchingPollComposer(QuickPoll Poll)
            : base(ServerPacketHeader.MatchingPollMessageComposer)
        {
            WriteString("MATCHING_POLL"); // May mPoll ID
            WriteInteger(Poll.Id); // May mPoll ID
            WriteInteger(3); // May mPoll Question ID
            WriteInteger(Poll.ReamigTime); //Duration

            WriteInteger(0); //Id
            WriteInteger(0); //Number
            WriteInteger(0); //Type

            WriteString(HttpUtility.HtmlDecode(Poll.Title)); //May title 2

            WriteInteger(-1); //selection_min

            WriteInteger(0); /*2);//len

            base.WriteString("1000");
            base.WriteString("1000");

            base.WriteString("1000");
            base.WriteString("10000");*/
        }
    }
}