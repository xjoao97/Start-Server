namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorSupportTicketResponseComposer : ServerPacket
    {
        public ModeratorSupportTicketResponseComposer(int Result)
            : base(ServerPacketHeader.ModeratorSupportTicketResponseMessageComposer)
        {
            WriteInteger(Result);
            WriteString("");
        }
    }
}