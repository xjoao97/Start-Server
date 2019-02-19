namespace Oblivion.Communication.Packets.Outgoing.Quests
{
    internal class QuestAbortedComposer : ServerPacket
    {
        public QuestAbortedComposer()
            : base(ServerPacketHeader.QuestAbortedMessageComposer)
        {
            WriteBoolean(false);
        }
    }
}