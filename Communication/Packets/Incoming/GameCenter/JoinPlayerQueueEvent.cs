#region

using System;
using System.Data;
using System.Linq;
using System.Text;
using Oblivion.Communication.Packets.Outgoing.GameCenter;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Games;

#endregion

namespace Oblivion.Communication.Packets.Incoming.GameCenter
{
    internal class JoinPlayerQueueEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            var gameId = packet.PopInt();

            GameData gameData;
            if (!OblivionServer.GetGame().GetGameDataManager().TryGetGame(gameId, out gameData)) return;
            session.SendMessage(new JoinQueueComposer(gameData.GameId));
            var habboId = session.GetHabbo().Id;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id FROM user_auth_food WHERE user_id = '" + habboId + "'");
                var data = dbClient.getTable();
                var count = data.Rows.Cast<DataRow>().Count(row => Convert.ToInt32(row["user_id"]) == habboId);
                if (count == 0)
                {
                    var ssoTicket = "Fastfood-" + GenerateSso(32) + "-" + session.GetHabbo().Id;
                    dbClient.RunQuery("INSERT INTO user_auth_food(user_id, auth_ticket) VALUES ('" + habboId +
                                      "', '" +
                                      ssoTicket + "')");
                    session.SendMessage(new LoadGameComposer(gameData, ssoTicket));
                }
                else
                {
                    dbClient.SetQuery("SELECT user_id,auth_ticket FROM user_auth_food WHERE user_id = " + habboId);
                    data = dbClient.getTable();
                    foreach (var ssoTicket in from DataRow dRow in data.Rows select dRow["auth_ticket"])
                        session.SendMessage(new LoadGameComposer(gameData, (string) ssoTicket));
                }
            }
        }

        private static string GenerateSso(int length)
        {
            var random = new Random();
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                result.Append(characters[random.Next(characters.Length)]);
            return result.ToString();
        }
    }
}