#region

using System.Data;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorUserInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var UserId = Packet.PopInt();

            DataRow User = null;
            DataRow Info = null;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`username`,`online`,`mail`,`ip_last`,`look`,`account_created`,`last_online` FROM `users` WHERE `id` = '" +
                    UserId + "' LIMIT 1");
                User = dbClient.GetRow();

                if (User == null)
                {
                    Session.SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("user_not_found"));
                    return;
                }

                dbClient.SetQuery(
                    "SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '" +
                    UserId + "' LIMIT 1");
                Info = dbClient.GetRow();
                if (Info == null)
                {
                    dbClient.RunFastQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + UserId + "')");
                    dbClient.SetQuery(
                        "SELECT `cfhs`,`cfhs_abusive`,`cautions`,`bans`,`trading_locked`,`trading_locks_count` FROM `user_info` WHERE `user_id` = '" +
                        UserId + "' LIMIT 1");
                    Info = dbClient.GetRow();
                }
            }


            Session.SendMessage(new ModeratorUserInfoComposer(User, Info));
        }
    }
}