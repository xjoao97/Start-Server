#region

using System.Collections.Generic;
using System.Data;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Pets.Locale
{
    public class PetLocale
    {
        private Dictionary<string, string[]> _values;

        public PetLocale()
        {
            _values = new Dictionary<string, string[]>();

            Init();
        }

        public void Init()
        {
            _values = new Dictionary<string, string[]>();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bots_pet_responses`");
                var Pets = dbClient.getTable();

                if (Pets != null)
                    foreach (DataRow Row in Pets.Rows)
                        _values.Add(Row[0].ToString(), Row[1].ToString().Split(';'));
            }
        }

        public string[] GetValue(string key)
        {
            string[] value;
            if (_values.TryGetValue(key, out value))
                return value;
            return new[] {"Unknown pet speach:" + key};
        }
    }
}