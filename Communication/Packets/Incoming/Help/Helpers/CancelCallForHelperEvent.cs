/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class CancelCallForHelperEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var call =  OblivionServer.GetGame().GetHelperManager().GetCall(Session);
             OblivionServer.GetGame().GetHelperManager().RemoveCall(call);
            Session.SendMessage(new CloseHelperSessionComposer());
            if (call.Helper != null)
                call.Helper.CancelCall();

            Session.SendMessage(new CloseHelperSessionComposer());
        }
    }
}*/

