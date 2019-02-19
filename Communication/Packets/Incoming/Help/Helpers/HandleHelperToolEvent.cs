/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class HandleHelperToolEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var onDuty = Packet.PopBoolean();
            var isGuide = Packet.PopBoolean();
            var isHelper = Packet.PopBoolean();
            var isGuardian = Packet.PopBoolean();
            if (onDuty)
                 OblivionServer.GetGame().GetHelperManager().AddHelper(Session, isHelper, isGuardian, isGuide);
            else
                 OblivionServer.GetGame().GetHelperManager().RemoveHelper(Session);

            Session.SendMessage(new HandleHelperToolComposer(onDuty));
        }
    }
}*/

