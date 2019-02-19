namespace Oblivion.Communication.Packets.Outgoing.Nux
{
    internal class NuxAlertComposer : ServerPacket
    {
        public NuxAlertComposer(string Message)
            : base(ServerPacketHeader.NuxAlertMessageComposer)
        {
            WriteString(Message);
        }
    }
}