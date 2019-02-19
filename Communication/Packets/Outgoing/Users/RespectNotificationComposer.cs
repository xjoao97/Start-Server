namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class RespectNotificationComposer : ServerPacket
    {
        public RespectNotificationComposer(int userID, int Respect)
            : base(ServerPacketHeader.RespectNotificationMessageComposer)
        {
            WriteInteger(userID);
            WriteInteger(Respect);
        }
    }
}