#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class GetNavigatorFlatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Categories = OblivionServer.GetGame().GetNavigator().GetEventCategories();

            Session.SendMessage(new NavigatorFlatCatsComposer(Categories));
        }
    }
}