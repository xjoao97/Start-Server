#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.FloorPlan
{
    internal class InitializeFloorPlanSessionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //Session.SendNotif("WARNING - THIS TOOL IS IN BETA, IT COULD CORRUPT YOUR ROOM IF YOU CONFIGURE THE MAP WRONG OR DISCONNECT YOU.");
        }
    }
}