#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class OpenHelpToolEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet) => Session.SendMessage(new OpenHelpToolComposer());
    }
}