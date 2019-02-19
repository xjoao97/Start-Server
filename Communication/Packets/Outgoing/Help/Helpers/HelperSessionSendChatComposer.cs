namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class HelperSessionSendChatComposer : ServerPacket
    {
        public HelperSessionSendChatComposer(int senderId, string message)
            : base(ServerPacketHeader.HelperSessionSendChatMessageComposer)
        {
            WriteString(message);
            WriteInteger(senderId);
        }
    }
}