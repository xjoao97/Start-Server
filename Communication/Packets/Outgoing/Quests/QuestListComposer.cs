#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Quests
{
    public class QuestListComposer : ServerPacket
    {
        public QuestListComposer(GameClient Session, bool Send, Dictionary<string, Quest> UserQuests)
            : base(ServerPacketHeader.QuestListMessageComposer)
        {
            WriteInteger(UserQuests.Count);

            // Active ones first
            foreach (var UserQuest in UserQuests.Where(UserQuest => UserQuest.Value != null))
                SerializeQuest(this, Session, UserQuest.Value, UserQuest.Key);

            // Dead ones last
            foreach (var UserQuest in UserQuests.Where(UserQuest => UserQuest.Value == null))
                SerializeQuest(this, Session, UserQuest.Value, UserQuest.Key);

            WriteBoolean(Send);
        }

        private static void SerializeQuest(ServerPacket Message, GameClient Session, Quest Quest, string Category)
        {
            if (Message == null || Session == null)
                return;

            var AmountInCat = OblivionServer.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(Category);
            var Number = Quest?.Number - 1 ?? AmountInCat;
            var UserProgress = Quest == null ? 0 : Session.GetHabbo().GetQuestProgress(Quest.Id);

            if (Quest != null && Quest.IsCompleted(UserProgress))
                Number++;

            Message.WriteString(Category);
            Message.WriteInteger(Quest == null ? 0 : (Quest.Category.Contains("xmas2012") ? 0 : Number));
            // Quest progress in this cat
            Message.WriteInteger(Quest == null ? 0 : Quest.Category.Contains("xmas2012") ? 0 : AmountInCat);
            // Total quests in this cat
            Message.WriteInteger(Quest?.RewardType ?? 3);
            // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
            Message.WriteInteger(Quest?.Id ?? 0); // Quest id
            Message.WriteBoolean(Quest != null && Session.GetHabbo().GetStats().QuestID == Quest.Id);
            // Quest started
            Message.WriteString(Quest == null ? string.Empty : Quest.ActionName);
            Message.WriteString(Quest == null ? string.Empty : Quest.DataBit);
            Message.WriteInteger(Quest?.Reward ?? 0);
            Message.WriteString(Quest == null ? string.Empty : Quest.Name);
            Message.WriteInteger(UserProgress); // Current progress
            Message.WriteInteger(Quest?.GoalData ?? 0); // Target progress
            Message.WriteInteger(Quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
            Message.WriteString("");
            Message.WriteString("");
            Message.WriteBoolean(true);
        }
    }
}