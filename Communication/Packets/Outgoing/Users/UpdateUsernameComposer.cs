namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class UpdateUsernameComposer : ServerPacket
    {
        public UpdateUsernameComposer(string User)
            : base(ServerPacketHeader.UpdateUsernameMessageComposer)
        {
            WriteInteger(0);
            WriteString(User);
            WriteInteger(0);
        }
    }
}