#region

using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class FollowFriendEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            var BuddyId = Packet.PopInt();
            if (BuddyId == 0 || BuddyId == Session.GetHabbo().Id)
                return;

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(BuddyId);
            if (Client == null || Client.GetHabbo() == null)
                return;

            if (!Client.GetHabbo().InRoom)
            {
                Session.SendMessage(new FollowFriendFailedComposer(2));
                Session.GetHabbo().GetMessenger().UpdateFriend(Client.GetHabbo().Id, Client, true);
                return;
            }
            if (Session.GetHabbo().CurrentRoom != null && Client.GetHabbo().CurrentRoom != null)
                if (Session.GetHabbo().CurrentRoom.RoomId == Client.GetHabbo().CurrentRoom.RoomId)
                    return;

            Session.SendMessage(new RoomForwardComposer(Client.GetHabbo().CurrentRoomId));
        }
    }
}