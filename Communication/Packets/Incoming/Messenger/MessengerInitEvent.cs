#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class MessengerInitEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            Session.GetHabbo().GetMessenger().OnStatusChanged(false);

            ICollection<MessengerBuddy> Friends = Session.GetHabbo().GetMessenger().GetFriends().Where(Buddy => Buddy != null && !Buddy.IsOnline).ToList();

            Session.SendMessage(new MessengerInitComposer());
            Session.SendMessage(new BuddyListComposer(Friends, Session.GetHabbo()));

            Session.GetHabbo().GetMessenger().ProcessOfflineMessages();
        }
    }
}