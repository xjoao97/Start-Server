#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    public class GetUserFlatCatsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
                return;

            var Categories = OblivionServer.GetGame().GetNavigator().GetFlatCategories();

            Session.SendMessage(new UserFlatCatsComposer(Categories, Session.GetHabbo().Rank));
        }
    }
}