namespace Oblivion.Communication.Packets.Outgoing.Talents
{
    internal class TalentLevelUpComposer : ServerPacket
    {
        public TalentLevelUpComposer()
            : base(ServerPacketHeader.TalentLevelUpMessageComposer)
        {
        }
    }
}