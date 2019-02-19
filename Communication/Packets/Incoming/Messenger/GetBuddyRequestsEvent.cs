#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class GetBuddyRequestsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            ICollection<MessengerRequest> Requests = Session.GetHabbo().GetMessenger().GetRequests().ToList();

            Session.SendMessage(new BuddyRequestsComposer(Requests));
        }
    }
}