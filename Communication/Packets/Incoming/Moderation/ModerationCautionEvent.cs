#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModerationCautionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_caution"))
                return;

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null || Client.GetHabbo() == null)
                return;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `cautions` = `cautions` + '1' WHERE `user_id` = '" +
                                  Client.GetHabbo().Id + "' LIMIT 1");
            }

            Client.SendNotification(Message);
        }
    }
}