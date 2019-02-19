namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class HelperSessionChatIsTypingComposer : ServerPacket
    {
        public HelperSessionChatIsTypingComposer(bool typing)
            : base(ServerPacketHeader.HelperSessionChatIsTypingMessageComposer)
        {
            WriteBoolean(typing);
        }
    }
}