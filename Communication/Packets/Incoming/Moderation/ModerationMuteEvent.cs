#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class ModerationMuteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null ||
                !Session.GetHabbo().GetPermissions().HasRight("mod_mute"))
                return;

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();
            double Length = Packet.PopInt() * 60;
            var Unknown1 = Packet.PopString();
            var Unknown2 = Packet.PopString();

            var Habbo = OblivionServer.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_mute") &&
                !Session.GetHabbo().GetPermissions().HasRight("mod_mute_any"))
            {
                Session.SendWhisper("Oops, you cannot mute that user.");
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `users` SET `time_muted` = '" + Length + "' WHERE `id` = '" + Habbo.Id +
                                  "' LIMIT 1");
            }

            if (Habbo.GetClient() != null)
            {
                Habbo.TimeMuted = Length;
                Habbo.GetClient().SendNotification("You have been muted by a moderator for " + Length + " seconds!");
            }
        }
    }
}