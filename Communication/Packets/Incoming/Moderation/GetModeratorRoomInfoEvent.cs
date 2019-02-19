#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorRoomInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var RoomId = Packet.PopInt();

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Data == null)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room))
                return;

            Session.SendMessage(new ModeratorRoomInfoComposer(Data,
                Room.GetRoomUserManager().GetRoomUserByHabbo(Data.OwnerName) != null));
        }
    }
}