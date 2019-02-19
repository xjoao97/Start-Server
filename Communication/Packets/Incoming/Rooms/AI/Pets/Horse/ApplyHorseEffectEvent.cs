#region

using Oblivion.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    internal class ApplyHorseEffectEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var ItemId = Packet.PopInt();
            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            var PetId = Packet.PopInt();

            RoomUser PetUser = null;
            if (!Room.GetRoomUserManager().TryGetPet(PetId, out PetUser))
                return;

            if (PetUser.PetData == null || PetUser.PetData.OwnerId != Session.GetHabbo().Id)
                return;

            if (Item.Data.InteractionType == InteractionType.HorseSaddle1)
            {
                PetUser.PetData.Saddle = 9;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '9' WHERE `id` = '" +
                                      PetUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id, false);
            }
            else if (Item.Data.InteractionType == InteractionType.HorseSaddle2)
            {
                PetUser.PetData.Saddle = 10;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `have_saddle` = '10' WHERE `id` = '" +
                                      PetUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id, false);
            }
            else if (Item.Data.InteractionType == InteractionType.HorseHairstyle)
            {
                var Parse = 100;
                var HairType = Item.GetBaseItem().ItemName.Split('_')[2];

                Parse = Parse + int.Parse(HairType);

                PetUser.PetData.PetHair = Parse;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `pethair` = '" + PetUser.PetData.PetHair +
                                      "' WHERE `id` = '" + PetUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id, false);
            }
            else if (Item.Data.InteractionType == InteractionType.HorseHairDye)
            {
                var HairDye = 48;
                var HairType = Item.GetBaseItem().ItemName.Split('_')[2];

                HairDye = HairDye + int.Parse(HairType);
                PetUser.PetData.HairDye = HairDye;

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `hairdye` = '" + PetUser.PetData.HairDye +
                                      "' WHERE `id` = '" + PetUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id, false);
            }
            else if (Item.Data.InteractionType == InteractionType.HorseBodyDye)
            {
                var Race = Item.GetBaseItem().ItemName.Split('_')[2];
                var Parse = int.Parse(Race);
                var RaceLast = 2 + Parse * 4 - 4;
                if (Parse == 13)
                    RaceLast = 61;
                else if (Parse == 14)
                    RaceLast = 65;
                else if (Parse == 15)
                    RaceLast = 69;
                else if (Parse == 16)
                    RaceLast = 73;
                PetUser.PetData.Race = RaceLast.ToString();

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `race` = '" + PetUser.PetData.Race + "' WHERE `id` = '" +
                                      PetUser.PetData.PetId + "' LIMIT 1");
                    dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + Item.Id + "' LIMIT 1");
                }

                //We only want to use this if we're successful. 
                Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id, false);
            }

            //Update the Pet and the Pet figure information.
            Room.SendMessage(new UsersComposer(PetUser));
            Room.SendMessage(new PetHorseFigureInformationComposer(PetUser));
        }
    }
}