#region

using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class FriendNotificationComposer : ServerPacket
    {
        public FriendNotificationComposer(int UserId, MessengerEventTypes type, string data)
            : base(ServerPacketHeader.FriendNotificationMessageComposer)
        {
            WriteString(UserId.ToString());
            WriteInteger(MessengerEventTypesUtility.GetEventTypePacketNum(type));
            WriteString(data);
        }
    }
}