namespace Oblivion.Communication.Packets.Outgoing.Help
{
    internal class SubmitBullyReportComposer : ServerPacket
    {
        public SubmitBullyReportComposer(int Result)
            : base(ServerPacketHeader.SubmitBullyReportMessageComposer)
        {
            WriteInteger(Result);
        }
    }
}