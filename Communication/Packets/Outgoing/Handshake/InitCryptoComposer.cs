namespace Oblivion.Communication.Packets.Outgoing.Handshake
{
    public class InitCryptoComposer : ServerPacket
    {
        public InitCryptoComposer()
            : base(ServerPacketHeader.InitCryptoMessageComposer)
        {
            WriteString("null");
            WriteString("disabled");
        }
    }
}