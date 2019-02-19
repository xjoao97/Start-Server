#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class UnbanUserFromRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Instance = Session.GetHabbo().CurrentRoom;
            if (Instance == null || !Instance.CheckRights(Session, true))
                return;

            var UserId = Packet.PopInt();
            var RoomId = Packet.PopInt();

            if (Instance.BannedUsers().Contains(UserId))
            {
                Instance.Unban(UserId);
                Session.SendMessage(new UnbanUserFromRoomComposer(RoomId, UserId));
            }
        }
    }
}