namespace Oblivion.Communication.Packets.Outgoing.Handshake
{
    internal class VideoOffersRewardsComposer : ServerPacket
    {
        public VideoOffersRewardsComposer(int Id, string Type, string Message)
            : base(ServerPacketHeader.VideoOffersRewardsMessageComposer)
        {
            WriteString(Type);
            WriteInteger(Id);
            WriteString(Message);
            WriteString("");
        }
    }
}