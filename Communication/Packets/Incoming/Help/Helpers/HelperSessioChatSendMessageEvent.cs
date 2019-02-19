/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class HelperSessioChatSendMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Element =  OblivionServer.GetGame().GetHelperManager().GetElement(Session);
            var message = Packet.PopString();
            if (Element.OtherElement != null)
            {
                Session.SendMessage(new HelperSessionSendChatComposer(Session.GetHabbo().Id, message));
                Element.OtherElement.Session.SendMessage(new HelperSessionSendChatComposer(Session.GetHabbo().Id,
                    message));
            }
            else
            {
                Session.SendMessage(new CallForHelperErrorComposer(0));
            }
        }
    }
}*/

