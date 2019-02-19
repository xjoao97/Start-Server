#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class FriendListUpdateEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
        }
    }
}