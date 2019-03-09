#region

using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Badges
{
    internal class SetActivatedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.GetHabbo().GetBadgeComponent().ResetSlots();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_badges` SET `badge_slot` = '0' WHERE `user_id` = @userId");
                dbClient.AddParameter("userId", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            for (var i = 0; i < 5; i++)
            {
                var Slot = Packet.PopInt();
                var Badge = Packet.PopString();

                if (Badge.Length == 0)
                    continue;

                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(Badge) || Slot < 1 || Slot > 5)
                    return;

                Session.GetHabbo().GetBadgeComponent().GetBadge(Badge).Slot = Slot;

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `user_badges` SET `badge_slot` = " + Slot +
                                      " WHERE `badge_id` = @badge AND `user_id` = '" + Session.GetHabbo().Id +
                                      "' LIMIT 1");
                    dbClient.AddParameter("badge", Badge);
                    dbClient.RunQuery();
                }
            }

            OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_BADGE);

            Room Room;

            if (Session.GetHabbo().InRoom &&
                OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                Session.GetHabbo().CurrentRoom.SendMessage(new HabboUserBadgesComposer(Session.GetHabbo()));
            else
                Session.SendMessage(new HabboUserBadgesComposer(Session.GetHabbo()));
        }
    }
}