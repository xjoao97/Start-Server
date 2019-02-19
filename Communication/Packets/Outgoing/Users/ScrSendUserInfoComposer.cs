namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class ScrSendUserInfoComposer : ServerPacket
    {
        public ScrSendUserInfoComposer()
            : base(ServerPacketHeader.ScrSendUserInfoMessageComposer)
        {
            var DisplayMonths = 0;
            var DisplayDays = 0;

            WriteString("habbo_club");
            WriteInteger(DisplayDays);
            WriteInteger(2);
            WriteInteger(DisplayMonths);
            WriteInteger(1);
            WriteBoolean(true); // hc
            WriteBoolean(true); // vip
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(495);
        }
    }
}