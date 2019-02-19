#region

using System.Collections.Generic;
using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Navigator;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class NewNavigatorSearchEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            var Category = packet.PopString();
            var Search = packet.PopString();

            ICollection<SearchResultList> Categories = new List<SearchResultList>();

            if (!string.IsNullOrEmpty(Search))
            {
                SearchResultList QueryResult;
                if (OblivionServer.GetGame().GetNavigator().TryGetSearchResultList(0, out QueryResult))
                    Categories.Add(QueryResult);
            }
            else
            {
                Categories = OblivionServer.GetGame().GetNavigator().GetCategorysForSearch(Category);
                if (Categories.Count == 0)
                {
                    //Are we going in deep?!
                    Categories = OblivionServer.GetGame().GetNavigator().GetResultByIdentifier(Category);
                    if (Categories.Count > 0)
                    {
                        session.SendMessage(new NavigatorSearchResultSetComposer(Category, Search, Categories, session,
                            2, 100));
                        return;
                    }
                }
            }

            session.SendMessage(new NavigatorSearchResultSetComposer(Category, Search, Categories, session));
        }
    }
}