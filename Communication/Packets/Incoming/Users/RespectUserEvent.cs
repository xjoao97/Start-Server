#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class RespectUserEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().InRoom || Session.GetHabbo().GetStats().DailyRespectPoints <= 0)
                return;

            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Packet.PopInt());
            if (User == null || User.GetClient() == null ||
                User.GetClient().GetHabbo().Id == Session.GetHabbo().Id || User.IsBot)
                return;

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_RESPECT);
            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_RespectGiven", 1);
            OblivionServer.GetGame()
                .GetAchievementManager()
                .ProgressAchievement(User.GetClient(), "ACH_RespectEarned", 1);

            Session.GetHabbo().GetStats().DailyRespectPoints -= 1;
            Session.GetHabbo().GetStats().RespectGiven += 1;
            User.GetClient().GetHabbo().GetStats().Respect += 1;

            Room.SendMessage(new RespectNotificationComposer(User.GetClient().GetHabbo().Id,
                User.GetClient().GetHabbo().GetStats().Respect));
            Room.SendMessage(new ActionComposer(ThisUser.VirtualId, 7));
        }
    }
}