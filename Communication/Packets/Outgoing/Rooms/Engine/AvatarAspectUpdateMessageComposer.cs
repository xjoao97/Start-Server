namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class AvatarAspectUpdateMessageComposer : ServerPacket
    {
        public AvatarAspectUpdateMessageComposer(string Figure, string Gender)
            : base(ServerPacketHeader.AvatarAspectUpdateMessageComposer)
        {
            WriteString(Figure);
            WriteString(Gender);
        }
    }
}