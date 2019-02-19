using Oblivion.HabboHotel.GameClients;

namespace Oblivion.Communication.Packets.Incoming.Rooms.Camera
{
    public class PublishPhotoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {//todo: publish camera
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User?.LastPhotoPreview == null)
                return;
        }
    }
}