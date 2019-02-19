#region

using Oblivion.Communication.Packets.Incoming;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets
{
    public interface IPacketEvent
    {
        void Parse(GameClient Session, ClientPacket Packet);
    }
}