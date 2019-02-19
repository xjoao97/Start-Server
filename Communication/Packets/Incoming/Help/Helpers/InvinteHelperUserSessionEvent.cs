/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class InvinteHelperUserSessionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Element =  OblivionServer.GetGame().GetHelperManager().GetElement(Session);
            var room = Session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            Element.OtherElement.Session.SendMessage(new HelperSessionInvinteRoomComposer(room.Id, room.Name));
            Session.SendMessage(new HelperSessionInvinteRoomComposer(room.Id, room.Name));
        }
    }
}*/

