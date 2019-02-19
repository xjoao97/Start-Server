#region

using System;
using System.Collections.Generic;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.HabboHotel.Catalog.Utilities;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Rooms.AI.Speech;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class CheckGnomeNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room?.CheckRights(Session) != true)
                return;

            var ItemId = Packet.PopInt();
            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item?.Data == null || Item.UserID != Session.GetHabbo().Id ||
                Item.Data.InteractionType != InteractionType.GnomeBox)
                return;

            var PetName = Packet.PopString();
            if (string.IsNullOrEmpty(PetName))
            {
                Session.SendMessage(new CheckGnomeNameComposer(PetName, 1));
                return;
            }

            var X = Item.GetX;
            var Y = Item.GetY;

            //Quickly delete it from the database.
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @ItemId LIMIT 1");
                dbClient.AddParameter("ItemId", Item.Id);
                dbClient.RunQuery();
            }

            //Remove the item.
            Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);

            //Apparently we need this for success.
            Session.SendMessage(new CheckGnomeNameComposer(PetName, 0));

            //Create the pet here.
            var Pet = PetUtility.CreatePet(Session.GetHabbo().Id, PetName, 26, "30", "ffffff");
            if (Pet == null)
            {
                Session.SendNotification("Oops, an error occoured. Please report this!");
                return;
            }

            var RndSpeechList = new List<RandomSpeech>();
            //var BotResponse = new List<BotResponse>();

            Pet.RoomId = Session.GetHabbo().CurrentRoomId;
            Pet.GnomeClothing = RandomClothing();

            //Update the pets gnome clothing.
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE `bots_petdata` SET `gnome_clothing` = @GnomeClothing WHERE `id` = @PetId LIMIT 1");
                dbClient.AddParameter("GnomeClothing", Pet.GnomeClothing);
                dbClient.AddParameter("PetId", Pet.PetId);
                dbClient.RunQuery();
            }

            //Make a RoomUser of the pet.
            Room.GetRoomUserManager()
                .DeployBot(
                    new RoomBot(Pet.PetId, Pet.RoomId, "pet", "freeroam", Pet.Name, "", Pet.Look, X, Y, 0, 0, 0, 0,
                        0, 0, ref RndSpeechList, "", 0, Pet.OwnerId, false, 0, false, 0), Pet);

            //Give the food.
//            ItemData PetFood;
            var PetFood = OblivionServer.GetGame().GetItemManager().GetItem(320);
            if (PetFood != null)
            {
                var Food = ItemFactory.CreateSingleItemNullable(PetFood, Session.GetHabbo(), "", "");
                if (Food != null)
                {
                    Session.GetHabbo().GetInventoryComponent().TryAddItem(Food);
                    Session.SendMessage(new FurniListNotificationComposer(Food.Id, 1));
                }
            }
        }

        private static string RandomClothing()
        {
            var Random = new Random();

            var RandomNumber = Random.Next(1, 6);
            switch (RandomNumber)
            {
                default:
                case 1:
                    return "5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                case 2:
                    return "5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                case 3:
                    return "5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                case 4:
                    return "5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                case 5:
                    return "5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24";
                case 6:
                    return "5 4 402 5 3 302 21 0 -1 7 1 101 12 2 201 17";
            }
        }
    }
}