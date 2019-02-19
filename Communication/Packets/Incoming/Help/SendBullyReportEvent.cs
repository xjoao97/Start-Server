#region

using Oblivion.Communication.Packets.Outgoing.Help;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help
{
    internal class SendBullyReportEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new SendBullyReportComposer());
    }
}