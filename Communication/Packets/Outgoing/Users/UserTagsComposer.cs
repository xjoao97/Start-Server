namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class UserTagsComposer : ServerPacket
    {
        public UserTagsComposer(int UserId)
            : base(ServerPacketHeader.UserTagsMessageComposer)
        {
            WriteInteger(UserId);
            WriteInteger(0); //Count of the tags.
            {
                //Append a string.
            }
        }
    }
}