#region

using Oblivion.Communication.Packets.Outgoing.Sound;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Sound
{
    internal class GetSongInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new TraxSongInfoComposer());
        }
    }
}