using System;

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Camera
{
    public class CameraPhotoPurchaseOkComposer : ServerPacket
    {
        public CameraPhotoPurchaseOkComposer()
            : base(ServerPacketHeader.CameraPurchaseOkComposer)
        {
        }
    }
}
