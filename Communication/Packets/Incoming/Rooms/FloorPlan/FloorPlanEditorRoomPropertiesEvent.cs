#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.FloorPlan;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.FloorPlan
{
    internal class FloorPlanEditorRoomPropertiesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var Model = Room?.GetGameMap().Model;
            if (Model == null)
                return;

            var FloorItems = Room.GetRoomItemHandler().GetFloor;

            Session.SendMessage(new FloorPlanFloorMapComposer(FloorItems));
            Session.SendMessage(new FloorPlanSendDoorComposer(Model.DoorX, Model.DoorY, Model.DoorOrientation));
            Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness,
                OblivionServer.EnumToBool(Room.Hidewall.ToString())));
        }
    }
}