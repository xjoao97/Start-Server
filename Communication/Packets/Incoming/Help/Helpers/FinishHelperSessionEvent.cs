/*#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Global;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class FinishHelperSessionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Voted = Packet.PopBoolean();
            var Element =  OblivionServer.GetGame().GetHelperManager().GetElement(Session);
            if (Element is HelperCase)
                if (Voted)
                    Element.OtherElement.Session.SendNotification("helper.tool.session.liked" + "%username%" + Session.GetHabbo().Username);
                        
                else
                    Element.OtherElement.Session.SendNotification("helper.tool.session.unliked" + "%username%" + Session.GetHabbo().Username);


            Element.Close();
        }
    }
}*/

