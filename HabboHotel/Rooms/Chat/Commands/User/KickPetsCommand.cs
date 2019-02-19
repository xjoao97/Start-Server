#region

using System.Drawing;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Pets;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class KickPetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_kickpets";

        public string Parameters => "";

        public string Description => "Expulse todos os pets da sala.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Opa, apenas o dono do quarto pode usar este comando!");
                return;
            }

            if (room.GetRoomUserManager().GetPets().Count > 0)
            {
                foreach (var Pet in room.GetRoomUserManager().GetUserList().ToList().Where(Pet => Pet != null))
                {
                    if (Pet.RidingHorse)
                    {
                        var userRiding = room.GetRoomUserManager().GetRoomUserByVirtualId(Pet.HorseID);
                        if (userRiding != null)
                        {
                            userRiding.RidingHorse = false;
                            userRiding.ApplyEffect(-1);
                            userRiding.MoveTo(new Point(userRiding.X + 1, userRiding.Y + 1));
                        }
                        else
                        {
                            Pet.RidingHorse = false;
                        }
                    }

                    Pet.PetData.RoomId = 0;
                    Pet.PetData.PlacedInRoom = false;

                    var pet = Pet.PetData;
                    if (pet != null)
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(
                                "UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '" +
                                pet.PetId + "' LIMIT 1");
                            dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + pet.experience +
                                              "', `energy` = '" + pet.Energy + "', `nutrition` = '" + pet.Nutrition +
                                              "', `respect` = '" + pet.Respect + "' WHERE `id` = '" + pet.PetId +
                                              "' LIMIT 1");
                        }

                    if (pet.OwnerId != session.GetHabbo().Id)
                    {
                        var target = OblivionServer.GetGame().GetClientManager().GetClientByUserID(pet.OwnerId);
                        if (target != null)
                        {
                            target.GetHabbo().GetInventoryComponent().TryAddPet(Pet.PetData);
                            room.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);

                            target.SendMessage(
                                new PetInventoryComposer(target.GetHabbo().GetInventoryComponent().GetPets()));
                            return;
                        }
                    }

                    session.GetHabbo().GetInventoryComponent().TryAddPet(Pet.PetData);
                    room.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
                    session.SendMessage(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
                }
                session.SendWhisper("Sucesso, todos os pets foram expulsos.");
            }
            else
            {
                session.SendWhisper("Hm? Será que realmente existem pets aqui?");
            }
        }
    }
}