#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class SetMessengerInviteStatusEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Status = Packet.PopBoolean();

            Session.GetHabbo().AllowMessengerInvites = Status;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `ignore_invites` = @MessengerInvites WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("MessengerInvites", OblivionServer.BoolToEnum(Status));
                dbClient.RunQuery();
            }
        }
    }
}