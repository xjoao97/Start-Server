namespace Oblivion.Communication.Packets.Outgoing.Handshake
{
    public class SecretKeyComposer : ServerPacket
    {
        public SecretKeyComposer()
            : base(ServerPacketHeader.SecretKeyMessageComposer)
        {
            WriteString("Disabled");
            WriteBoolean(false);
        }
    }
}