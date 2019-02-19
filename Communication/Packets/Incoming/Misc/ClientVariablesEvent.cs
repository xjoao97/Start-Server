#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Misc
{
    internal class ClientVariablesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GordanPath = Packet.PopString();
            var ExternalVariables = Packet.PopString();
        }
    }
}