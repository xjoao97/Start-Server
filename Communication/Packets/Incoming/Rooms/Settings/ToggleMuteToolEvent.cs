#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class ToggleMuteToolEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            Room.RoomMuted = !Room.RoomMuted;

            var roomUsers = Room.GetRoomUserManager().GetRoomUsers();
            foreach (var roomUser in roomUsers.ToList().Where(roomUser => roomUser?.GetClient() != null))
                roomUser.GetClient()
                    .SendWhisper(Room.RoomMuted ? "Este quarto foi silenciado." : "O quarto não está mais silenciado.");

            Room.SendMessage(new RoomMuteSettingsComposer(Room.RoomMuted));
        }
    }
}