namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class FlatAccessDeniedComposer : ServerPacket
    {
        public FlatAccessDeniedComposer(string Username)
            : base(ServerPacketHeader.FlatAccessDeniedMessageComposer)
        {
            WriteString(Username);
        }
    }
}