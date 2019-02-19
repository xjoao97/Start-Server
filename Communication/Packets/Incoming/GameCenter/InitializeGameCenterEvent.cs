#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.GameCenter
{
    internal class InitializeGameCenterEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
        }
    }
}