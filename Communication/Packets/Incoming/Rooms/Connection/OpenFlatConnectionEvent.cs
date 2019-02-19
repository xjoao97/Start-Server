#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Connection
{
    public class OpenFlatConnectionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            var RoomId = Packet.PopInt();

            var Password = Packet.PopString();

            Session.GetHabbo().PrepareRoom(RoomId, Password);
        }
    }
}