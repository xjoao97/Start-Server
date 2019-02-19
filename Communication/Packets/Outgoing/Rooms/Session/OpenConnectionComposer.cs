namespace Oblivion.Communication.Packets.Outgoing.Rooms.Session
{
    internal class OpenConnectionComposer : ServerPacket
    {
        public OpenConnectionComposer()
            : base(ServerPacketHeader.OpenConnectionMessageComposer)
        {
        }
    }
}