#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Avatar
{
    internal class ChangeMottoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Oops, you're currently muted - you cannot change your motto.");
                return;
            }

            if ((DateTime.Now - Session.GetHabbo().LastMottoUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().MottoUpdateWarnings += 1;
                if (Session.GetHabbo().MottoUpdateWarnings >= 25)
                    Session.GetHabbo().SessionMottoBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionMottoBlocked)
                return;

            Session.GetHabbo().LastMottoUpdateTime = DateTime.Now;

            var newMotto = StringCharFilter.Escape(Packet.PopString().Trim());

            if (newMotto.Length > 38)
                newMotto = newMotto.Substring(0, 38);

            if (newMotto == Session.GetHabbo().Motto)
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                newMotto = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(newMotto);

            Session.GetHabbo().Motto = newMotto;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `motto` = @motto WHERE `id` = '" + Session.GetHabbo().Id +
                                  "' LIMIT 1");
                dbClient.AddParameter("motto", newMotto);
                dbClient.RunQuery();
            }

            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_MOTTO);
            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Motto", 1);

            if (Session.GetHabbo().InRoom)
            {
                var Room = Session.GetHabbo().CurrentRoom;
                if (Room == null)
                    return;

                var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User == null || User.GetClient() == null)
                    return;

                Room.SendMessage(new UserChangeComposer(User, false));
            }
        }
    }
}