using System;

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Camera
{
    public class CameraPhotoPreviewComposer : ServerPacket
    {
        public CameraPhotoPreviewComposer(string ImageUrl)
            : base(ServerPacketHeader.CameraPhotoPreviewComposer)
        {
            WriteString(ImageUrl);
        }
    }
}