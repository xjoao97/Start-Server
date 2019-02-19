namespace Oblivion.Communication.Packets.Outgoing.Rooms.Session
{
    internal class CantConnectComposer : ServerPacket
    {
        public CantConnectComposer(int Error)
            : base(ServerPacketHeader.CantConnectMessageComposer)
        {
            WriteInteger(Error);
        }
    }
}