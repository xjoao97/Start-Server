﻿#region

using System;
using Oblivion.Communication.Packets.Outgoing.Pets;
using Oblivion.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;

#endregion

namespace Oblivion.HabboHotel.Rooms.AI
{
    public class Pet
    {
        public int AnyoneCanRide;
        public string Color;
        public double CreationStamp;
        public DatabaseUpdateState DBState;

        public int Energy;
        public int experience;

        public int[] experienceLevels =
        {
            100, 200, 400, 600, 1000, 1300, 1800, 2400, 3200, 4300, 7200, 8500, 10100,
            13300, 17500, 23000, 51900, 75000, 128000, 150000
        };

        public string GnomeClothing;
        public int HairDye;
        public string Name;
        public int Nutrition;
        public int OwnerId;
        public int PetHair;
        public int PetId;
        public bool PlacedInRoom;
        public string Race;
        public int Respect;
        public int RoomId;
        public int Saddle;

        public int Type;
        public int VirtualId;
        public int X;
        public int Y;
        public double Z;

        public Pet(int PetId, int OwnerId, int RoomId, string Name, int Type, string Race, string Color, int experience,
            int Energy, int Nutrition, int Respect, double CreationStamp, int X, int Y, double Z, int Saddle,
            int Anyonecanride, int Dye, int PetHer, string GnomeClothing)
        {
            this.PetId = PetId;
            this.OwnerId = OwnerId;
            this.RoomId = RoomId;
            this.Name = Name;
            this.Type = Type;
            this.Race = Race;
            this.Color = Color;
            this.experience = experience;
            this.Energy = Energy;
            this.Nutrition = Nutrition;
            this.Respect = Respect;
            this.CreationStamp = CreationStamp;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            PlacedInRoom = false;
            DBState = DatabaseUpdateState.Updated;
            this.Saddle = Saddle;
            AnyoneCanRide = Anyonecanride;
            PetHair = PetHer;
            HairDye = Dye;
            this.GnomeClothing = GnomeClothing;
        }

        public Room Room
        {
            get
            {
                if (!IsInRoom)
                    return null;

                Room _room;

                if (OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out _room))
                    return _room;
                return null;
            }
        }

        public bool IsInRoom => RoomId > 0;

        public int Level
        {
            get
            {
                for (var level = 0; level < experienceLevels.Length; ++level)
                    if (experience < experienceLevels[level])
                        return level + 1;
                return experienceLevels.Length;
            }
        }

        public static int MaxLevel => 20;

        public int experienceGoal => experienceLevels[Level - 1];

        public static int MaxEnergy => 100;

        public static int MaxNutrition => 150;

        public int Age => Convert.ToInt32(Math.Floor((OblivionServer.GetUnixTimestamp() - CreationStamp) / 86400));

        public string Look => Type + " " + Race + " " + Color + " " + GnomeClothing;

        public string OwnerName => OblivionServer.GetGame().GetClientManager().GetNameById(OwnerId);

        public void OnRespect()
        {
            Respect++;
            Room.SendMessage(new RespectPetNotificationMessageComposer(this));

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;

            if (experience <= 150000)
                Addexperience(10);
        }

        public void Addexperience(int Amount)
        {
            experience = experience + Amount;

            if (experience > 150000)
            {
                experience = 150000;

                Room?.SendMessage(new AddExperiencePointsComposer(PetId, VirtualId, Amount));

                return;
            }

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;

            if (Room == null) return;
            Room.SendMessage(new AddExperiencePointsComposer(PetId, VirtualId, Amount));

            if (experience >= experienceGoal)
                Room.SendMessage(new ChatComposer(VirtualId, "*leveled up to level " + Level + " *", 0, 0));
        }

        public void PetEnergy(bool Add)
        {
            int MaxE;
            if (Add)
            {
                if (Energy == 100) // If Energy is 100, no point.
                    return;

                if (Energy > 85)
                    MaxE = MaxEnergy - Energy;
                else
                    MaxE = 10;
            }
            else
            {
                MaxE = 15; // Remove Max Energy as 15
            }

            if (MaxE <= 4)
                MaxE = 15;

            var r = OblivionServer.GetRandomNumber(4, MaxE);

            if (!Add)
            {
                Energy = Energy - r;

                if (Energy < 0)
                {
                    Energy = 1;
                }
            }
            else

            {
                Energy = Energy + r;
            }


            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;
        }
    }

    public enum DatabaseUpdateState
    {
        Updated,
        NeedsUpdate,
        NeedsInsert
    }
}