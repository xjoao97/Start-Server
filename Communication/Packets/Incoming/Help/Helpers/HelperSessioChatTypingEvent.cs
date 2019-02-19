/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class HelperSessioChatTypingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var element =  OblivionServer.GetGame().GetHelperManager().GetElement(Session);
            element?.OtherElement?.Session.SendMessage(new HelperSessionChatIsTypingComposer(Packet.PopBoolean()));
        }
    }
}*/

