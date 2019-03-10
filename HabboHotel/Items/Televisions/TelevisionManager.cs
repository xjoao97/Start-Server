#region

using System;
using System.Collections.Generic;
using System.Data;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Items.Televisions
{
    public class TelevisionManager
    {
        private static readonly ILog log =
            LogManager.GetLogger("Oblivion.HabboHotel.Items.Televisions.TelevisionManager");

        public Dictionary<int, TelevisionItem> _televisions;

        public TelevisionManager()
        {
            _televisions = new Dictionary<int, TelevisionItem>();

            Init();
        }


        public ICollection<TelevisionItem> TelevisionList => _televisions.Values;

        public void Init()
        {
            if (_televisions.Count > 0)
                _televisions.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `items_youtube` ORDER BY `id` DESC");
                var getData = dbClient.GetTable();

                if (getData != null)
                    foreach (DataRow Row in getData.Rows)
                        _televisions.Add(Convert.ToInt32(Row["id"]),
                            new TelevisionItem(Convert.ToInt32(Row["id"]), Row["youtube_id"].ToString(),
                                Row["title"].ToString(), Row["description"].ToString(),
                                OblivionServer.EnumToBool(Row["enabled"].ToString())));
            }


            log.Info("Television Items -> LOADED");
        }
    }
}