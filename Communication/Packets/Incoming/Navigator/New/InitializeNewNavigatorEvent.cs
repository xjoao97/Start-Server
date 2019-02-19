#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class InitializeNewNavigatorEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var TopLevelItems = OblivionServer.GetGame().GetNavigator().GetTopLevelItems();

            Session.SendMessage(new NavigatorMetaDataParserComposer(TopLevelItems));
            Session.SendMessage(new NavigatorLiftedRoomsComposer());
            Session.SendMessage(new NavigatorCollapsedCategoriesComposer());
            Session.SendMessage(new NavigatorPreferencesComposer());
        }
    }
}