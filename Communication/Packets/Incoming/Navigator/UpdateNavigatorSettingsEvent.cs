#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class UpdateNavigatorSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var roomID = Packet.PopInt();
            if (roomID == 0)
                return;

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(roomID);
            if (Data == null)
                return;

            Session.GetHabbo().HomeRoom = roomID;
            Session.SendMessage(new NavigatorSettingsComposer(roomID));
        }
    }
}