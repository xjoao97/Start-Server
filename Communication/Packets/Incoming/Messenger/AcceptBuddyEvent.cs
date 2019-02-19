#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class AcceptBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            var Amount = Packet.PopInt();
            if (Amount > 50)
                Amount = 50;
            else if (Amount < 0)
                return;

            for (var i = 0; i < Amount; i++)
            {
                var RequestId = Packet.PopInt();

                MessengerRequest Request;
                if (!Session.GetHabbo().GetMessenger().TryGetRequest(RequestId, out Request))
                    continue;

                if (Request.To != Session.GetHabbo().Id)
                    return;

                if (!Session.GetHabbo().GetMessenger().FriendshipExists(Request.To))
                    Session.GetHabbo().GetMessenger().CreateFriendship(Request.From);

                Session.GetHabbo().GetMessenger().HandleRequest(RequestId);
            }
        }
    }
}