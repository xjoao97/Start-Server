namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class UnreadForumThreadPostsComposer : ServerPacket
    {
        public UnreadForumThreadPostsComposer(int count)
            : base(ServerPacketHeader.UnreadForumThreadPostsMessageComposer)
        {
            WriteInteger(count);
        }
    }
}