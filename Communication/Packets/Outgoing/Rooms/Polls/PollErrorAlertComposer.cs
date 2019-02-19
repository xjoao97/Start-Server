namespace Oblivion.Communication.Packets.Outgoing.Rooms.Polls
{
    internal class PollErrorAlertComposer : ServerPacket
    {
        public PollErrorAlertComposer()
            : base(ServerPacketHeader.PollErrorAlertMessageComposer)
        {
        }
    }
}