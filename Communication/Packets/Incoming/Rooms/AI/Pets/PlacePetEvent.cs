#region

using System.Collections.Generic;
using log4net;
using Oblivion.Communication.Packets.Outgoing.Inventory.Pets;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Rooms.AI.Speech;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.AI.Pets
{
    internal class PlacePetEvent : IPacketEvent
    {
        private static readonly ILog log =
            LogManager.GetLogger("Oblivion.Communication.Packets.Incoming.Rooms.AI.Pets.PlacePetEvent");

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (Room.AllowPets == 0 && !Room.CheckRights(Session, true) || !Room.CheckRights(Session, true))
            {
                Session.SendMessage(new RoomErrorNotifComposer(1));
                return;
            }

            if (Room.GetRoomUserManager().PetCount > OblivionStaticGameSettings.RoomPetPlacementLimit)
            {
                Session.SendMessage(new RoomErrorNotifComposer(2)); //5 = I have too many.
                return;
            }

            Pet Pet;
            if (!Session.GetHabbo().GetInventoryComponent().TryGetPet(Packet.PopInt(), out Pet))
                return;

            if (Pet == null)
                return;

            if (Pet.PlacedInRoom)
            {
                Session.SendNotification("This pet is already in the room?");
                return;
            }

            var X = Packet.PopInt();
            var Y = Packet.PopInt();

            if (!Room.GetGameMap().CanWalk(X, Y, false))
            {
                Session.SendMessage(new RoomErrorNotifComposer(4));
                return;
            }

            RoomUser OldPet;
            if (Room.GetRoomUserManager().TryGetPet(Pet.PetId, out OldPet))
                Room.GetRoomUserManager().RemoveBot(OldPet.VirtualId, false);

            Pet.X = X;
            Pet.Y = Y;

            Pet.PlacedInRoom = true;
            Pet.RoomId = Room.RoomId;

            var RndSpeechList = new List<RandomSpeech>();
            var RoomBot = new RoomBot(Pet.PetId, Pet.RoomId, "pet", "freeroam", Pet.Name, "", Pet.Look, X, Y, 0, 0, 0, 0,
                0, 0, ref RndSpeechList, "", 0, Pet.OwnerId, false, 0, false, 0);

            Room.GetRoomUserManager().DeployBot(RoomBot, Pet);

            Pet.DBState = DatabaseUpdateState.NeedsUpdate;
            Room.GetRoomUserManager().UpdatePets();

            Pet ToRemove;
            if (!Session.GetHabbo().GetInventoryComponent().TryRemovePet(Pet.PetId, out ToRemove))
            {
                log.Error("Erro ao remover o pet: " + ToRemove.PetId);
                return;
            }

            Session.SendMessage(new PetInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetPets()));
        }
    }
}