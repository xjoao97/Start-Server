/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class VisitHelperUserSessionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Element =  OblivionServer.GetGame().GetHelperManager().GetElement(Session);
            var other = Element.OtherElement;
            if (other == null)
                return;
            if (other.Session.GetHabbo().CurrentRoom == null)
                return;

            Element.Session.SendMessage(new HelperSessionVisiteRoomComposer(other.Session.GetHabbo().CurrentRoom.Id));
        }
    }
}*/

