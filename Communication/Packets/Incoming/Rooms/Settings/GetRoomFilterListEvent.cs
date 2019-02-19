#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Settings;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class GetRoomFilterListEvent : IPacketEvent
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

            Session.SendMessage(new GetRoomFilterListComposer(Instance));
            OblivionServer.GetGame()
                .GetAchievementManager()
                .ProgressAchievement(Session, "ACH_SelfModRoomFilterSeen", 1);
        }
    }
}