#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class GetGuestRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var roomID = Packet.PopInt();

            var roomData = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(roomID);
            if (roomData == null)
                return;

            var isLoading = Packet.PopInt() == 1;
            var checkEntry = Packet.PopInt() == 1;

            Session.SendMessage(new GetGuestRoomResultComposer(Session, roomData, isLoading, checkEntry));
        }
    }
}