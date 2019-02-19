#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Room = OblivionServer.GetGame().GetRoomManager().LoadRoom(Packet.PopInt());
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            Session.SendMessage(new RoomSettingsDataComposer(Room));
        }
    }
}