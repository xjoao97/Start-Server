#region

using System;
using System.Collections.Generic;
using System.Data;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Styles
{
    public sealed class ChatStyleManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Rooms.Chat.Styles.ChatStyleManager");

        private readonly Dictionary<int, ChatStyle> _styles;

        public ChatStyleManager()
        {
            _styles = new Dictionary<int, ChatStyle>();
        }

        public void Init()
        {
            _styles.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_chat_styles`;");
                var Table = dbClient.getTable();

                if (Table != null)
                    foreach (DataRow Row in Table.Rows)
                        try
                        {
                            if (!_styles.ContainsKey(Convert.ToInt32(Row["id"])))
                                _styles.Add(Convert.ToInt32(Row["id"]),
                                    new ChatStyle(Convert.ToInt32(Row["id"]), Convert.ToString(Row["name"]),
                                        Convert.ToString(Row["required_right"])));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Unable to load ChatBubble for ID [" + Convert.ToInt32(Row["id"]) + "]", ex);
                        }
            }

            log.Info("Loaded " + _styles.Count + " chat styles.");
        }

        public bool TryGetStyle(int Id, out ChatStyle Style) => _styles.TryGetValue(Id, out Style);
    }
}