#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Camera;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Camera
{
    internal class GetCameraPrice : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
            =>
                Session.SendMessage(
                    new CameraPriceComposer(OblivionServer.GetGame().GetCameraManager().PurchaseCoinsPrice,
                        OblivionServer.GetGame().GetCameraManager().PurchaseDucketsPrice,
                        OblivionServer.GetGame().GetCameraManager().PublishDucketsPrice));
    }
}