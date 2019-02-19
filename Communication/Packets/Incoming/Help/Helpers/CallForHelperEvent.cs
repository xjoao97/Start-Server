/*#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Global;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class CallForHelperEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var category = Packet.PopInt();
            var message = Packet.PopString();

            var helper =  OblivionServer.GetGame().GetHelperManager().GetHelper(Session);
            if (helper != null)
            {
                Session.SendNotification("Você já é um ajudande");
                Session.SendMessage(new CloseHelperSessionComposer());
                return;
            }

            var call =  OblivionServer.GetGame().GetHelperManager().AddCall(Session, message, category);
            var helpers =  OblivionServer.GetGame().GetHelperManager().GetHelpersToCase(call).FirstOrDefault();

            if (helpers != null)
            {
                 OblivionServer.GetGame().GetHelperManager().InvinteHelpCall(helpers, call);
                Session.SendMessage(new CallForHelperWindowComposer(false, call));
                return;
            }

            Session.SendMessage(new CallForHelperErrorComposer(1));
        }
    }
}*/

