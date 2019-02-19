#region

using Oblivion.HabboHotel.Cache;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class NewBuddyRequestComposer : ServerPacket
    {
        public NewBuddyRequestComposer(UserCache Habbo)
            : base(ServerPacketHeader.NewBuddyRequestMessageComposer)
        {
            WriteInteger(Habbo.Id);
            WriteString(Habbo.Username);
            WriteString(Habbo.Look);
        }
    }
}