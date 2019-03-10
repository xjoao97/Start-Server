#region

using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class OpenPlayerProfileEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var userID = Packet.PopInt();
            var IsMe = Packet.PopBoolean();

            var targetData = OblivionServer.GetHabboById(userID);
            if (targetData == null)
            {
                Session.SendNotification("O usuário não foi encontrado.");
                return;
            }

            var Groups = OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(targetData.Id);

            int friendCount;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                dbClient.AddParameter("userid", userID);
                friendCount = dbClient.getInteger();
            }

            /*
             *  dbClient.SetQuery("SELECT `badge_id`,`badge_slot` FROM user_badges WHERE `user_id` = '" + UserId + "'");
            dBadges = dbClient.getTable();
            var badges = (from DataRow dRow in dBadges.Rows
                          select new Badge(Convert.ToString(dRow["badge_id"]), Convert.ToInt32(dRow["badge_slot"]))).ToList();

    */

            Session.SendMessage(new ProfileInformationComposer(targetData, Session, Groups, friendCount));
        }
    }
}
 