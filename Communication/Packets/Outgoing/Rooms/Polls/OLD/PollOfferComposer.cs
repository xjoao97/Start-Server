namespace Oblivion.Communication.Packets.Outgoing.Rooms.Polls
{
    internal class PollOfferComposer : ServerPacket
    {
        public PollOfferComposer()
            : base(1074)
        {
            WriteInteger(111141); //Room Id
            WriteString("CLIENT_NPS");
            WriteString("Customer Satisfaction Poll");
            WriteString("Give us your opinion!");
        }
    }
}