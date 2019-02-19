namespace Oblivion.Communication.Packets.Outgoing.LandingView
{
    internal class HallOfFameCodeDataComposer : ServerPacket
    {
        public HallOfFameCodeDataComposer()
            : base(ServerPacketHeader.UpdateHallOfFameCodeDataMessageComposer)
        {
            WriteString("hall");
            WriteString("halloffame");
        }
    }
}