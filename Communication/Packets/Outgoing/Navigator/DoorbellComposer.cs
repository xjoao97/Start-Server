namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class DoorbellComposer : ServerPacket
    {
        public DoorbellComposer(string Username)
            : base(ServerPacketHeader.DoorbellMessageComposer)
        {
            WriteString(Username);
        }
    }
}