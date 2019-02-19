#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class GetRoomRightsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Instance = Session.GetHabbo().CurrentRoom;
            if (Instance == null)
                return;

            if (!Instance.CheckRights(Session))
                return;


            Session.SendMessage(new RoomRightsListComposer(Instance));
        }
    }
}