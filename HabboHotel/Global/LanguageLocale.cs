#region

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Global
{
    public class LanguageLocale
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Global.LanguageLocale");
        private readonly HybridDictionary _values;

        public LanguageLocale()
        {
            _values = new HybridDictionary();

            Init();
        }

        public void Init()
        {
            if (_values.Count > 0)
                _values.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_locale`");
                var Table = dbClient.getTable();

                if (Table != null)
                    foreach (DataRow Row in Table.Rows)
                        _values.Add(Row["key"].ToString(), Row["value"].ToString());
            }

            log.Info("Language Locale Manager -> CARREGADO");
        }


        public string TryGetValue(string value)
            => _values.Contains(value) ? (string) _values[value] : "Missing language locale for [" + value + "]";
    }
}