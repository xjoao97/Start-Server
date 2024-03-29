﻿#region

using Oblivion.Communication.Packets.Outgoing.Pets;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.AI.Pets
{
    internal class RespectPetEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom || Session.GetHabbo().GetStats() == null ||
                Session.GetHabbo().GetStats().DailyPetRespectPoints == 0)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            var PetId = Packet.PopInt();

            RoomUser Pet = null;
            if (!Session.GetHabbo().CurrentRoom.GetRoomUserManager().TryGetPet(PetId, out Pet))
            {
                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                var TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(PetId);

                //Check some values first, please!
                if (TargetUser?.GetClient() == null || TargetUser.GetClient().GetHabbo() == null)
                    return;

                if (TargetUser.GetClient().GetHabbo().Id == Session.GetHabbo().Id)
                {
                    Session.SendWhisper(
                        "Você não pode fazer carinho consigo mesmo. (Você não perderá este respeito, basta relogar)");
                    return;
                }

                //And boom! Let us send some respect points.
                OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_RESPECT);
                OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_RespectGiven", 1);
                OblivionServer.GetGame()
                    .GetAchievementManager()
                    .ProgressAchievement(TargetUser.GetClient(), "ACH_RespectEarned", 1);

                //Take away from pet respect points, just in-case users abuse this..
                Session.GetHabbo().GetStats().DailyPetRespectPoints -= 1;
                Session.GetHabbo().GetStats().RespectGiven += 1;
                TargetUser.GetClient().GetHabbo().GetStats().Respect += 1;

                //Apply the effect.
                ThisUser.CarryItemID = 999999999;
                ThisUser.CarryTimer = 5;

                //Send the magic out.
                Room.SendMessage(new RespectPetNotificationMessageComposer(TargetUser.GetClient().GetHabbo(),
                    TargetUser));
                Room.SendMessage(new CarryObjectComposer(ThisUser.VirtualId, ThisUser.CarryItemID));
                return;
            }

            if (Pet?.PetData == null || Pet.RoomId != Session.GetHabbo().CurrentRoomId)
                return;

            Session.GetHabbo().GetStats().DailyPetRespectPoints -= 1;
            OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_PetRespectGiver", 1);

            ThisUser.CarryItemID = 999999999;
            ThisUser.CarryTimer = 5;
            Pet.PetData.OnRespect();
            Room.SendMessage(new CarryObjectComposer(ThisUser.VirtualId, ThisUser.CarryItemID));
        }
    }
}