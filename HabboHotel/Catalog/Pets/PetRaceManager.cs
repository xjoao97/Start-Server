#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Oblivion.HabboHotel.Catalog.Pets
{
    public class PetRaceManager
    {
        private readonly List<PetRace> _races = new List<PetRace>();

        public void Init()
        {
            if (_races.Count > 0)
                _races.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `catalog_pet_races`");
                var Table = dbClient.GetTable();

                if (Table == null) return;
                foreach (
                    var Race in
                    Table.Rows.Cast<DataRow>()
                        .Select(Row => new PetRace(Convert.ToInt32(Row["raceid"]), Convert.ToInt32(Row["color1"]),
                            Convert.ToInt32(Row["color2"]), Convert.ToString(Row["has1color"]) == "1",
                            Convert.ToString(Row["has2color"]) == "1")).Where(Race => !_races.Contains(Race)))
                    _races.Add(Race);
            }
        }

       public List<PetRace> GetRacesForRaceId(int RaceId) => _races.Where(Race => Race.RaceId == RaceId).ToList();
    }
}