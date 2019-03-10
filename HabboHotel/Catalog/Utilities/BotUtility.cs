#region

using System;
using System.Data;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Users.Inventory.Bots;

#endregion

namespace Oblivion.HabboHotel.Catalog.Utilities
{
    public static class BotUtility
    {
        public static Bot CreateBot(ItemData Data, int OwnerId)
        {
            DataRow botData;
            CatalogBot cataBot;
            if (!OblivionServer.GetGame().GetCatalog().TryGetBot(Data.Id, out cataBot))
                return null;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO bots (`user_id`,`name`,`motto`,`look`,`gender`,`ai_type`) VALUES ('" +
                                  OwnerId + "', '" + cataBot.Name + "', '" + cataBot.Motto + "', '" + cataBot.Figure +
                                  "', '" + cataBot.Gender + "', '" + cataBot.AIType + "')");
                var Id = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery(
                    "SELECT `id`,`user_id`,`name`,`motto`,`look`,`gender` FROM `bots` WHERE `user_id` = '" + OwnerId +
                    "' AND `id` = '" + Id + "' LIMIT 1");
                botData = dbClient.GetRow();
            }

            return new Bot(Convert.ToInt32(botData["id"]), Convert.ToInt32(botData["user_id"]),
                Convert.ToString(botData["name"]), Convert.ToString(botData["motto"]), Convert.ToString(botData["look"]),
                Convert.ToString(botData["gender"]));
        }


        public static BotAIType GetAIFromString(string Type)
        {
            switch (Type)
            {
                case "pet":
                    return BotAIType.PET;
                case "generic":
                    return BotAIType.GENERIC;
                case "bartender":
                    return BotAIType.BARTENDER;
                case "visitor_logger":
                    return BotAIType.VISITOR_LOGGER;
                case "casino_bot":
                    return BotAIType.CASINO_BOT;
                case "game_text":
                    return BotAIType.GAME_TEXT;
                default:
                    return BotAIType.GENERIC;
            }
        }
    }
}