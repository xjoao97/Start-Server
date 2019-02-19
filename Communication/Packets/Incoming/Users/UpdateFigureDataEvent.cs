#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class UpdateFigureDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            var Gender = Packet.PopString().ToUpper();
            var Look = OblivionServer.GetGame().GetAntiMutant().RunLook(Packet.PopString());

            if (Look == Session.GetHabbo().Look)
                return;

            if ((DateTime.Now - Session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().ClothingUpdateWarnings += 1;
                if (Session.GetHabbo().ClothingUpdateWarnings >= 25)
                    Session.GetHabbo().SessionClothingBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionClothingBlocked)
                return;

            Session.GetHabbo().LastClothingUpdateTime = DateTime.Now;

            string[] AllowedGenders = {"M", "F"};
            if (!AllowedGenders.Contains(Gender))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Sorry, you chose an invalid gender."));
                return;
            }

            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);

            Session.GetHabbo().Look = OblivionServer.FilterFigure(Look);
            Session.GetHabbo().Gender = Gender.ToLower();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("look", Look);
                dbClient.AddParameter("gender", Gender);
                dbClient.RunQuery();
            }

            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_AvatarLooks", 1);
            Session.SendMessage(new AvatarAspectUpdateMessageComposer(Look, Gender)); //esto
            if (Session.GetHabbo().Look.Contains("ha-1006"))
                OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WEAR_HAT);

            if (!Session.GetHabbo().InRoom) return;
            var RoomUser =
                Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (RoomUser == null) return;
            Session.SendMessage(new UserChangeComposer(RoomUser, true));
            Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
        }
    }
}