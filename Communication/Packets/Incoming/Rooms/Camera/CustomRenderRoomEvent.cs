using System;

using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.GameClients;
using Oblivion.Communication.Packets.Outgoing.Rooms.Camera;
using Oblivion.HabboHotel.Camera;

namespace Oblivion.Communication.Packets.Incoming.Rooms.Camera
{
    public class CustomRenderRoomEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;

            RoomUser User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            int photoId;

            if (!int.TryParse(Packet.PopString(), out photoId) || photoId < 0)
            {
                return;
            }

            CameraPhotoPreview preview = OblivionServer.GetGame().GetCameraManager().GetPreview(photoId);

            if (preview == null || preview.CreatorId != Session.GetHabbo().Id)
            {
                return;
            }

            User.LastPhotoPreview = preview;
            Session.SendMessage(
                new CameraPhotoPreviewComposer(
                    OblivionServer.GetGame()
                        .GetCameraManager()
                        .GetPath(CameraPhotoType.PREVIEW, preview.Id, preview.CreatorId)));
        }
    }
}