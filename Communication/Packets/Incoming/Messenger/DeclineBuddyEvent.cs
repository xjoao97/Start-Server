#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class DeclineBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            var DeclineAll = Packet.PopBoolean();
            var Amount = Packet.PopInt();

            if (!DeclineAll)
            {
                var RequestId = Packet.PopInt();
                Session.GetHabbo().GetMessenger().HandleRequest(RequestId);
            }
            else
            {
                Session.GetHabbo().GetMessenger().HandleAllRequests();
            }
        }
    }
}