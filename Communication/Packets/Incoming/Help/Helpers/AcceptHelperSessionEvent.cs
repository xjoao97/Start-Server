/*#region

using Oblivion.Communication.Packets.Outgoing.Help.Helpers;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help.Helpers
{
    internal class AcceptHelperSessionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Accepted = Packet.PopBoolean();
            var Helper =  OblivionServer.GetGame().GetHelperManager().GetHelper(Session);

            if (Helper == null)
            {
                Session.SendMessage(new CloseHelperSessionComposer());
                return;
            }

            if (Accepted)
                Helper.Accept();
            else
                Helper.Decline();
        }
    }
}*/

