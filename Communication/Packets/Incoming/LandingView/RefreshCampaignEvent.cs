#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.LandingView;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.LandingView
{
    internal class RefreshCampaignEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            try
            {
                var parseCampaings = Packet.PopString();
                if (parseCampaings.Contains("gamesmaker"))
                    return;

                var campaingName = "";
                var parser = parseCampaings.Split(';');

                foreach (
                    var data in from t in parser where !string.IsNullOrEmpty(t) && !t.EndsWith(",") select t.Split(','))
                    campaingName = data[1];
                Session.SendMessage(
                    new HallOfFameComposer(OblivionServer.GetGame().GetLandingManager().GetHallOfFameUsers()));

                Session.SendMessage(new CampaignComposer(parseCampaings, campaingName));
            }
            catch
            {
            }
        }
    }
}