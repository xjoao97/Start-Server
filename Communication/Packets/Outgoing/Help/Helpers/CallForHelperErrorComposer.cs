namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class CallForHelperErrorComposer : ServerPacket
    {
        public CallForHelperErrorComposer(int errorCode)
            : base(ServerPacketHeader.CallForHelperErrorMessageComposer)
        {
            WriteInteger(errorCode);
        }
    }
}