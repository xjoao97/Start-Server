#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class GetRoomBannedUsersEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Instance = Session.GetHabbo().CurrentRoom;
            if (Instance == null || !Instance.CheckRights(Session, true))
                return;

            if (Instance.BannedUsers().Count > 0)
                Session.SendMessage(new GetRoomBannedUsersComposer(Instance));
        }
    }
}