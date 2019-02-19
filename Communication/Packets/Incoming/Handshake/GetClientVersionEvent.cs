#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Handshake
{
    public class GetClientVersionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Build = Packet.PopString();

            if (OblivionServer.SwfRevision != Build)
                OblivionServer.SwfRevision = Build;
        }
    }
}