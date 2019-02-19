#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class SetChatPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var ChatPreference = Packet.PopBoolean();

            Session.GetHabbo().ChatPreference = ChatPreference;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `chat_preference` = @chatPreference WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("chatPreference", OblivionServer.BoolToEnum(ChatPreference));
                dbClient.RunQuery();
            }
        }
    }
}