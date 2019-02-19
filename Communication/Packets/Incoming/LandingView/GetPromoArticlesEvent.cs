#region

using Oblivion.Communication.Packets.Outgoing.LandingView;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.LandingView
{
    internal class GetPromoArticlesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var LandingPromotions = OblivionServer.GetGame().GetLandingManager().GetPromotionItems();
            Session.SendMessage(new PromoArticlesComposer(LandingPromotions));
            Session.SendMessage(new HallOfFameComposer(OblivionServer.GetGame().GetLandingManager().GetHallOfFameUsers()));
            Session.SendMessage(new HallOfFameCodeDataComposer());
        }
    }
}