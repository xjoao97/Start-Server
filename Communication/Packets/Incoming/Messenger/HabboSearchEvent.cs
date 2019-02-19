#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class HabboSearchEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            var Query = StringCharFilter.Escape(Packet.PopString().Replace("%", ""));
            if (Query.Length < 1 || Query.Length > 100)
                return;

            var Friends = new List<SearchResult>();
            var OthersUsers = new List<SearchResult>();

            var Results = SearchResultFactory.GetSearchResult(Query);
            foreach (var Result in Results.ToList())
                if (Session.GetHabbo().GetMessenger().FriendshipExists(Result.UserId))
                    Friends.Add(Result);
                else
                    OthersUsers.Add(Result);

            Session.SendMessage(new HabboSearchResultComposer(Friends, OthersUsers));
        }
    }
}