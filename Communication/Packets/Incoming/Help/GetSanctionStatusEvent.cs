#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help
{
    internal class GetSanctionStatusEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //Session.SendMessage(new SanctionStatusComposer());
        }
    }
}