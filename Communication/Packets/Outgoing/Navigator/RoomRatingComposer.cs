namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class RoomRatingComposer : ServerPacket
    {
        public RoomRatingComposer(int Score, bool CanVote)
            : base(ServerPacketHeader.RoomRatingMessageComposer)
        {
            WriteInteger(Score);
            WriteBoolean(CanVote);
        }
    }
}