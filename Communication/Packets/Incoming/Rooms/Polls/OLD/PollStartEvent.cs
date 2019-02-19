#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Polls;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Polls
{
    internal class PollStartEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new PollContentsComposer());
        }
    }
}