#region

using System;
using System.Data;
using System.Text;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class UserInfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_user_info";

        public string Parameters => "%username%";

        public string Description => "Veja as informações de algum usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Digite o nome do usuário.");
                return;
            }

            DataRow userData;
            DataRow userInfo;
            var username = Params[1];

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`username`,`mail`,`rank`,`motto`,`credits`,`activity_points`,`vip_points`,`gotw_points`,`online`,`rank_vip` FROM users WHERE `username` = @Username LIMIT 1");
                dbClient.AddParameter("Username", username);
                userData = dbClient.GetRow();
            }

            if (userData == null)
            {
                session.SendNotification("Hey, não há nenhum usuário no banco de dados com esse nome de usuário (" +
                                         username + ")!");
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(userData["id"]) +
                                  "' LIMIT 1");
                userInfo = dbClient.GetRow();
                if (userInfo == null)
                {
                    dbClient.RunFastQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + Convert.ToInt32(userData["id"]) +
                                      "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(userData["id"]) +
                                      "' LIMIT 1");
                    userInfo = dbClient.GetRow();
                }
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(username);

            //var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToDouble(userInfo["trading_locked"]));

            var habboInfo = new StringBuilder();
            habboInfo.Append("Informações:\r");
            habboInfo.Append("ID: " + Convert.ToInt32(userData["id"]) + "\r");
            habboInfo.Append("Rank: " + Convert.ToInt32(userData["rank"]) + "\r");
            habboInfo.Append("Email: " + Convert.ToString(userData["mail"]) + "\r");
            habboInfo.Append("Online: " + (targetClient != null ? "Sim" : "Não") + "\r\r");

            habboInfo.Append("Informações financeiras:\r");
            habboInfo.Append("Moedas: " + Convert.ToInt32(userData["credits"]) + "\r");
            habboInfo.Append("duckets: " + Convert.ToInt32(userData["activity_points"]) + "\r");
            habboInfo.Append("Conchas: " + Convert.ToInt32(userData["vip_points"]) + "\r");
            habboInfo.Append("Moedas promocionais: " + Convert.ToInt32(userData["gotw_points"]) + "\r");

            habboInfo.Append("Informação <b>STAFF</b>:\r");
            habboInfo.Append("Banimentos: " + Convert.ToInt32(userInfo["bans"]) + "\r");

            if (targetClient != null)
            {
                habboInfo.Append("Informação Atual:\r");
                if (!targetClient.GetHabbo().InRoom)
                {
                    habboInfo.Append("Atualmente em nenhum quarto\r");
                }
                else
                {
                    habboInfo.Append("Quarto: " + targetClient.GetHabbo().CurrentRoom.Name + " (" +
                                     targetClient.GetHabbo().CurrentRoom.RoomId + ")\r");
                    habboInfo.Append("Dono do quarto: " + targetClient.GetHabbo().CurrentRoom.OwnerName + "\r");
                    habboInfo.Append("Número de visitantes: " + targetClient.GetHabbo().CurrentRoom.UserCount + "/" +
                                     targetClient.GetHabbo().CurrentRoom.UsersMax);
                }
            }
            session.SendNotification(habboInfo.ToString());
        }
    }
}