#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModerationTradeLockEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_trade_lock"))
                return;

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();
            double Days = Packet.PopInt() / 1440;
            var Unknown1 = Packet.PopString();
            var Unknown2 = Packet.PopString();

            var Length = OblivionServer.GetUnixTimestamp() + Days * 86400;

            var Habbo = OblivionServer.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_trade_lock") &&
                !Session.GetHabbo().GetPermissions().HasRight("mod_trade_lock_any"))
            {
                Session.SendWhisper("Oops, you cannot trade lock another user ranked 5 or higher.");
                return;
            }

            if (Days < 1)
                Days = 1;

            if (Days > 365)
                Days = 365;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("UPDATE `user_info` SET `trading_locked` = '" + Length +
                                  "', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '" +
                                  Habbo.Id + "' LIMIT 1");
            }

            if (Habbo.GetClient() != null)
            {
                Habbo.TradingLockExpiry = Length;
                Habbo.GetClient()
                    .SendNotification("You have been trade banned for " + Days + " day(s)!\r\rReason:\r\r" + Message);
            }
        }
    }
}