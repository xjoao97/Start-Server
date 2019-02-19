namespace Oblivion.Communication.Packets.Outgoing.Talents
{
    internal class TalentTrackLevelComposer : ServerPacket
    {
        public TalentTrackLevelComposer()
            : base(ServerPacketHeader.TalentTrackLevelMessageComposer)
        {
            WriteString("citizenship");
            WriteInteger(0);
            WriteInteger(4);
        }
    }
}