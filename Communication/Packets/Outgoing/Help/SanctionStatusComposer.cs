

namespace Oblivion.Communication.Packets.Outgoing.Help
{
    internal class SanctionStatusComposer : ServerPacket
    {
        public SanctionStatusComposer()
            : base(ServerPacketHeader.SanctionStatusMessageComposer)
        {
            WriteBoolean(true);
            WriteBoolean(false);
            WriteString("ALERT");
            WriteInteger(0);
            WriteInteger(30);
            WriteString("cfh.reason.EMPTY");
            WriteString("2016-07-17 16:33 (GMT +0000)");
            WriteInteger(720);
            WriteString("ALERT");
            WriteInteger(0);
            WriteInteger(30);
            WriteString("");
            WriteBoolean(false);
        }
    }
}