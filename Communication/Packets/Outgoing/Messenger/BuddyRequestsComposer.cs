#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class BuddyRequestsComposer : ServerPacket
    {
        public BuddyRequestsComposer(ICollection<MessengerRequest> Requests)
            : base(ServerPacketHeader.BuddyRequestsMessageComposer)
        {
            WriteInteger(Requests.Count);
            WriteInteger(Requests.Count);

            foreach (var Request in Requests)
            {
                WriteInteger(Request.From);
                WriteString(Request.Username);

                var User = OblivionServer.GetGame().GetCacheManager().GenerateUser(Request.From);
                WriteString(User != null ? User.Look : "");
            }
        }
    }
}