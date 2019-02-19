#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help
{
    internal class OnBullyClickEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //I am a very boring packet.
        }
    }
}