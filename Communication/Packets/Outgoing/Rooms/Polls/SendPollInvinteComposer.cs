#region

using Oblivion.HabboHotel.Rooms.Polls;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Polls
{
    internal class SendPollInvinteComposer : ServerPacket
    {
        public SendPollInvinteComposer(Poll Poll)
            : base(ServerPacketHeader.SendPollInvinteMessageComposer)
        {
            WriteInteger(Poll.Id);
            WriteString("10000");
            WriteString("10000");
            WriteString(Poll.Title + " - " + Poll.Desc);
        }
    }
}