#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Users.Inventory.Bots
{
    internal class BotLoader
    {
        public static List<Bot> GetBotsForUser(int UserId)
        {
            var B = new List<Bot>();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`user_id`,`name`,`motto`,`look`,`gender`FROM `bots` WHERE `user_id` = '" + UserId +
                    "' AND `room_id` = '0' AND `ai_type` != 'pet'");
                var dBots = dbClient.getTable();

                if (dBots != null)
                    B.AddRange(from DataRow dRow in dBots.Rows
                        select
                        new Bot(Convert.ToInt32(dRow["id"]), Convert.ToInt32(dRow["user_id"]),
                            Convert.ToString(dRow["name"]), Convert.ToString(dRow["motto"]),
                            Convert.ToString(dRow["look"]), Convert.ToString(dRow["gender"])));
            }
            return B;
        }
    }
}