#region

using System;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms.AI;

#endregion

namespace Oblivion.HabboHotel.Catalog.Utilities
{
    public static class PetUtility
    {

        public static bool CheckPetName(string PetName)
        {
            if (PetName.Length < 1 || PetName.Length > 16)
                return false;

            return OblivionServer.IsValidAlphaNumeric(PetName);
        }

        public static Pet CreatePet(int UserId, string Name, int Type, string Race, string Color)
        {
            var pet = new Pet(0, UserId, 0, Name, Type, Race, Color, 0, 100, 100, 0, OblivionServer.GetUnixTimestamp(),
                0, 0,
                0.0, 0, 0, 0, -1, "-1");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO bots (user_id,name, ai_type) VALUES (" + pet.OwnerId + ",@" + pet.PetId +
                                  "name, 'pet')");
                dbClient.AddParameter(pet.PetId + "name", pet.Name);
                pet.PetId = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery(
                    "INSERT INTO bots_petdata (id,type,race,color,experience,energy,createstamp) VALUES (" + pet.PetId +
                    ", " + pet.Type + ",@" + pet.PetId + "race,@" + pet.PetId + "color,0,100,UNIX_TIMESTAMP())");
                dbClient.AddParameter(pet.PetId + "race", pet.Race);
                dbClient.AddParameter(pet.PetId + "color", pet.Color);
                dbClient.RunQuery();
            }
            return pet;
        }
    }
}