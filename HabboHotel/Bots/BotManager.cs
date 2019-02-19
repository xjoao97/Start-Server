#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Rooms.AI.Responses;

#endregion

namespace Oblivion.HabboHotel.Bots
{
    public class BotManager
    {
        private readonly List<BotResponse> _responses;

        public BotManager()
        {
            _responses = new List<BotResponse>();

            Init();
        }

        public void Init()
        {
            if (_responses.Count > 0)
                _responses.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `bot_ai`,`chat_keywords`,`response_text`,`response_mode`,`response_beverage` FROM `bots_responses`");
                var BotResponses = dbClient.getTable();

                if (BotResponses != null)
                    foreach (DataRow Response in BotResponses.Rows)
                        _responses.Add(new BotResponse(Convert.ToString(Response["bot_ai"]),
                            Convert.ToString(Response["chat_keywords"]), Convert.ToString(Response["response_text"]),
                            Response["response_mode"].ToString(), Convert.ToString(Response["response_beverage"])));
            }
        }

        public BotResponse GetResponse(BotAIType AiType, string Message)
            =>
                _responses.Where(X => X.AiType == AiType)
                    .ToList()
                    .FirstOrDefault(Response => Response.KeywordMatched(Message));
    }
}