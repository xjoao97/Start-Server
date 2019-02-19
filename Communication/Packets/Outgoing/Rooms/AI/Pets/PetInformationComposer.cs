#region

using System;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.AI.Pets
{
    internal class PetInformationComposer : ServerPacket
    {
        public PetInformationComposer(Pet Pet)
            : base(ServerPacketHeader.PetInformationMessageComposer)
        {
            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Pet.RoomId, out Room))
                return;

            WriteInteger(Pet.PetId);
            WriteString(Pet.Name);
            WriteInteger(Pet.Level);
            WriteInteger(Pet.MaxLevel);
            WriteInteger(Pet.experience);
            WriteInteger(Pet.experienceGoal);
            WriteInteger(Pet.Energy);
            WriteInteger(Pet.MaxEnergy);
            WriteInteger(Pet.Nutrition);
            WriteInteger(Pet.MaxNutrition);
            WriteInteger(Pet.Respect);
            WriteInteger(Pet.OwnerId);
            WriteInteger(Pet.Age);
            WriteString(Pet.OwnerName);
            WriteInteger(1); //3 on hab
            WriteBoolean(Pet.Saddle > 0);
            WriteBoolean(false);
            WriteInteger(0); //5 on hab
            WriteInteger(Pet.AnyoneCanRide); // Anyone can ride horse
            WriteInteger(0);
            WriteInteger(0); //512 on hab
            WriteInteger(0); //1536
            WriteInteger(0); //2560
            WriteInteger(0); //3584
            WriteInteger(0);
            WriteString("");
            WriteBoolean(false);
            WriteInteger(-1); //255 on hab
            WriteInteger(-1);
            WriteInteger(-1);
            WriteBoolean(false);
        }

        public PetInformationComposer(Habbo Habbo)
            : base(ServerPacketHeader.PetInformationMessageComposer)
        {
            WriteInteger(Habbo.Id);
            WriteString(Habbo.Username);
            WriteInteger(Habbo.Rank);
            WriteInteger(10);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(100);
            WriteInteger(100);
            WriteInteger(100);
            WriteInteger(100);
            WriteInteger(Habbo.GetStats().Respect);
            WriteInteger(Habbo.Id);
            WriteInteger(Convert.ToInt32(Math.Floor((OblivionServer.GetUnixTimestamp() - Habbo.AccountCreated) / 86400)));
            //How?
            WriteString(Habbo.Username);
            WriteInteger(1); //3 on hab
            WriteBoolean(false);
            WriteBoolean(false);
            WriteInteger(0); //5 on hab
            WriteInteger(0); // Anyone can ride horse
            WriteInteger(0);
            WriteInteger(0); //512 on hab
            WriteInteger(0); //1536
            WriteInteger(0); //2560
            WriteInteger(0); //3584
            WriteInteger(0);
            WriteString("");
            WriteBoolean(false);
            WriteInteger(-1); //255 on hab
            WriteInteger(-1);
            WriteInteger(-1);
            WriteBoolean(false);
        }
    }
}