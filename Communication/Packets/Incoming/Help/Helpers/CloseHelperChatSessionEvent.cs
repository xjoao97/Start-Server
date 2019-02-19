/*#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class CloseHelperChatSessionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Element =  OblivionServer.GetGame().GetHelperManager().GetElement(Session);

            if (Element != null)
            {
                Element.End();
                if (Element.OtherElement != null)
                    Element.OtherElement.End();
            }
        }
    }
}*/

