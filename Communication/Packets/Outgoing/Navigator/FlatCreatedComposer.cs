namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class FlatCreatedComposer : ServerPacket
    {
        public FlatCreatedComposer(int roomID, string roomName)
            : base(ServerPacketHeader.FlatCreatedMessageComposer)
        {
            WriteInteger(0);
            WriteString(roomName);
        }
    }
}