#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Users.Clothing.Parts;

#endregion

namespace Oblivion.HabboHotel.Users.Clothing
{
    public sealed class ClothingComponent
    {
        /// <summary>
        ///     Effects stored by ID > Effect.
        /// </summary>
        private readonly ConcurrentDictionary<int, ClothingParts> _allClothing =
            new ConcurrentDictionary<int, ClothingParts>();

        private Habbo _habbo;

        public ICollection<ClothingParts> GetClothingAllParts => _allClothing.Values;

        /// <summary>
        ///     Initializes the EffectsComponent.
        /// </summary>
        /// <param name="UserId"></param>
        public bool Init(Habbo Habbo)
        {
            if (_allClothing.Count > 0)
                return false;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`part_id`,`part` FROM `user_clothing` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", Habbo.Id);
                var GetClothing = dbClient.GetTable();

                if (GetClothing != null)
                    foreach (DataRow Row in GetClothing.Rows)
                        _allClothing.TryAdd(Convert.ToInt32(Row["part_id"]),
                            new ClothingParts(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["part_id"]),
                                Convert.ToString(Row["part"])));
            }

            _habbo = Habbo;
            return true;
        }

        public void AddClothing(string ClothingName, List<int> PartIds)
        {
            foreach (var PartId in PartIds.ToList().Where(PartId => !_allClothing.ContainsKey(PartId)))
            {
                int NewId;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(
                        "INSERT INTO `user_clothing` (`user_id`,`part_id`,`part`) VALUES (@UserId, @PartId, @Part)");
                    dbClient.AddParameter("UserId", _habbo.Id);
                    dbClient.AddParameter("PartId", PartId);
                    dbClient.AddParameter("Part", ClothingName);
                    NewId = Convert.ToInt32(dbClient.InsertQuery());
                }

                _allClothing.TryAdd(PartId, new ClothingParts(NewId, PartId, ClothingName));
            }
        }

        public bool TryGet(int PartId, out ClothingParts ClothingPart)
            => _allClothing.TryGetValue(PartId, out ClothingPart);

        /// <summary>
        ///     Disposes the ClothingComponent.
        /// </summary>
        public void Dispose() => _allClothing.Clear();
    }
}