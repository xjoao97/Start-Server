#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class SetUserFocusPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var FocusPreference = Packet.PopBoolean();

            Session.GetHabbo().FocusPreference = FocusPreference;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `focus_preference` = @focusPreference WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("focusPreference", OblivionServer.BoolToEnum(FocusPreference));
                dbClient.RunQuery();
            }
        }
    }
}