namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class NuxNotificationComposer : ServerPacket
    {
        public NuxNotificationComposer(string Message)
            : base(ServerPacketHeader.NuxNotificationMessageComposer)
        {
            WriteInteger(3958885);
            WriteString(Message);
            WriteBoolean(false);
            WriteInteger(1013474668);
        }
    }
}