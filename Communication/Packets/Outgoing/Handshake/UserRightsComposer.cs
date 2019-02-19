namespace Oblivion.Communication.Packets.Outgoing.Handshake
{
    public class UserRightsComposer : ServerPacket
    {
        public UserRightsComposer(int Rank)
            : base(ServerPacketHeader.UserRightsMessageComposer)
        {
            WriteInteger(2); //Club level
            WriteInteger(Rank);
            WriteBoolean(true); //Is an ambassador
        }
    }
}