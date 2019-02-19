#region

using System;
using System.Drawing;
using System.Linq;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Rooms.AI.Types
{
    public class PetBot : BotAI
    {
        private int ActionTimer;
        private int EnergyTimer;
        private int SpeechTimer;

        public PetBot(int VirtualId)
        {
            SpeechTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            ActionTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + VirtualId);
            EnergyTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
        }

        private void RemovePetStatus()
        {
            var Pet = GetRoomUser();
            if (Pet == null) return;
            foreach (var KVP in Pet.Statusses.ToList().Where(KVP => Pet.Statusses.ContainsKey(KVP.Key)))
                Pet.Statusses.Remove(KVP.Key);
        }

        public override void OnSelfEnterRoom()
        {
            var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
            if (GetRoomUser() != null)
                GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
        }

        public override void OnSelfLeaveRoom(bool Kicked)
        {
        }


        public override void OnUserEnterRoom(RoomUser User)
        {
            if (User.GetClient() == null || User.GetClient().GetHabbo() == null) return;
            var Pet = GetRoomUser();
            if (Pet == null) return;
            if (User.GetClient().GetHabbo().Username != Pet.PetData.OwnerName) return;
            var Speech =
                OblivionServer.GetGame()
                    .GetChatManager()
                    .GetPetLocale()
                    .GetValue("welcome.speech.pet" + Pet.PetData.Type);
            var rSpeech = Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)];
            Pet.Chat(rSpeech, false);
        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
        }

        public override void OnTimerTick()
        {
            var Pet = GetRoomUser();
            if (Pet == null)
                return;

            #region Speech

            if (SpeechTimer <= 0)
            {
                if (Pet.PetData.DBState != DatabaseUpdateState.NeedsInsert)
                    Pet.PetData.DBState = DatabaseUpdateState.NeedsUpdate;

                {
                    RemovePetStatus();

                    var Speech =
                        OblivionServer.GetGame()
                            .GetChatManager()
                            .GetPetLocale()
                            .GetValue("speech.pet" + Pet.PetData.Type);
                    var rSpeech = Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)];

                    if (rSpeech.Length != 3)
                        Pet.Chat(rSpeech, false);
                    else
                        Pet.Statusses.Add(rSpeech, TextHandling.GetString(Pet.Z));
                }
                SpeechTimer = OblivionServer.GetRandomNumber(20, 120);
            }
            else
            {
                SpeechTimer--;
            }

            #endregion

            #region Actions

            if (ActionTimer <= 0)
                try
                {
                    RemovePetStatus();
                    ActionTimer = RandomNumber.GenerateRandom(15, 40 + GetRoomUser().PetData.VirtualId);
                    if (!GetRoomUser().RidingHorse)
                    {
                        // Remove Status
                        RemovePetStatus();

                        var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
                        if (GetRoomUser().CanWalk)
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                    }
                }
                catch (Exception e)
                {
                    Logging.HandleException(e, "PetBot.OnTimerTick");
                }
            else
                ActionTimer--;

            #endregion

            #region Energy

            if (EnergyTimer <= 0)
            {
                RemovePetStatus(); // Remove Status

                Pet.PetData.PetEnergy(true); // Add Energy

                EnergyTimer = RandomNumber.GenerateRandom(30, 120); // 2 Min Max
            }
            else
            {
                EnergyTimer--;
            }

            #endregion
        }

        #region Commands

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (User == null)
                return;

            var Pet = GetRoomUser();
            if (Pet == null)
                return;

            if (Pet.PetData.DBState != DatabaseUpdateState.NeedsInsert)
                Pet.PetData.DBState = DatabaseUpdateState.NeedsUpdate;

            if (Message.ToLower().Equals(Pet.PetData.Name.ToLower()))
            {
                Pet.SetRot(Rotation.Calculate(Pet.X, Pet.Y, User.X, User.Y), false);
                return;
            }

            if (Message.ToLower().StartsWith(Pet.PetData.Name.ToLower() + " ") &&
                string.Equals(User.GetClient().GetHabbo().Username, Pet.PetData.OwnerName,
                    StringComparison.CurrentCultureIgnoreCase) ||
                Message.ToLower().StartsWith(Pet.PetData.Name.ToLower() + " ") &&
                OblivionServer.GetGame()
                    .GetChatManager()
                    .GetPetCommands()
                    .TryInvoke(Message.Substring(Pet.PetData.Name.ToLower().Length + 1)) == 8)
            {
                var Command = Message.Substring(Pet.PetData.Name.ToLower().Length + 1);

                var r = RandomNumber.GenerateRandom(1, 8); // Made Random
                if (Pet.PetData.Energy > 10 && r < 6 || Pet.PetData.Level > 15 ||
                    OblivionServer.GetGame().GetChatManager().GetPetCommands().TryInvoke(Command) == 8)
                {
                    RemovePetStatus(); // Remove Status

                    switch (OblivionServer.GetGame().GetChatManager().GetPetCommands().TryInvoke(Command))
                    {
                            // TODO - Level you can use the commands at...

                            #region free

                        case 1:
                            RemovePetStatus();

                            //int randomX = OblivionServer.GetRandomNumber(0, GetRoom().Model.MapSizeX);
                            //int randomY = OblivionServer.GetRandomNumber(0, GetRoom().Model.MapSizeY);
                            var nextCoord = GetRoom().GetGameMap().getRandomWalkableSquare();
                            Pet.MoveTo(nextCoord.X, nextCoord.Y);

                            Pet.PetData.Addexperience(5); // Give XP
                            // Pet.PetData.Energy -= 5;
                            break;

                            #endregion

                            #region here

                        case 2:
                        case 22:

                            RemovePetStatus();

                            var NewX = User.X;
                            var NewY = User.Y;

                            ActionTimer = 30; // Reset ActionTimer

                            #region Rotation

                            switch (User.RotBody)
                            {
                                case 4:
                                    NewY = User.Y + 1;
                                    break;
                                case 0:
                                    NewY = User.Y - 1;
                                    break;
                                case 6:
                                    NewX = User.X - 1;
                                    break;
                                case 2:
                                    NewX = User.X + 1;
                                    break;
                                case 3:
                                    NewX = User.X + 1;
                                    NewY = User.Y + 1;
                                    break;
                                case 1:
                                    NewX = User.X + 1;
                                    NewY = User.Y - 1;
                                    break;
                                case 7:
                                    NewX = User.X - 1;
                                    NewY = User.Y - 1;
                                    break;
                                case 5:
                                    NewX = User.X - 1;
                                    NewY = User.Y + 1;
                                    break;
                            }

                            #endregion

                            Pet.PetData.Addexperience(10); // Give XP

                            Pet.MoveTo(NewX, NewY);
                            break;

                            #endregion

                            #region sit

                        case 3:
                            // Remove Status
                            RemovePetStatus();

                            Pet.PetData.Addexperience(10); // Give XP

                            // Add Status
                            Pet.Statusses.Add("sit", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            ActionTimer = 25;
                            EnergyTimer = 10;
                            break;

                            #endregion

                            #region lay

                        case 4:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status
                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            ActionTimer = 30;
                            EnergyTimer = 5;
                            break;

                            #endregion

                            #region dead

                        case 5:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status 
                            Pet.Statusses.Add("ded", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            SpeechTimer = 45;
                            ActionTimer = 30;

                            break;

                            #endregion

                            #region sleep

                        case 6:
                            // Remove Status
                            RemovePetStatus();

                            Pet.Chat("ZzzZZZzzzzZzz", false);
                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            EnergyTimer = 5;
                            SpeechTimer = 30;
                            ActionTimer = 45;
                            break;

                            #endregion

                            #region jump

                        case 7:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status 
                            Pet.Statusses.Add("jmp", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            EnergyTimer = 5;
                            SpeechTimer = 10;
                            ActionTimer = 5;
                            break;

                            #endregion

                            #region breed

                        case 46:

                            break;

                            #endregion

                        default:
                            var Speech =
                                OblivionServer.GetGame().GetChatManager().GetPetLocale().GetValue("pet.unknowncommand");

                            Pet.Chat(Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)], false);
                            break;
                    }
                    Pet.PetData.PetEnergy(false); // Remove Energy
                }
                else
                {
                    RemovePetStatus(); // Remove Status

                    var UserRiding = GetRoom().GetRoomUserManager().GetRoomUserByVirtualId(Pet.HorseID);

                    if (UserRiding.RidingHorse)
                    {
                        Pet.Chat("Getof my sit", false);
                        UserRiding.RidingHorse = false;
                        Pet.RidingHorse = false;
                        UserRiding.ApplyEffect(-1);
                        UserRiding.MoveTo(new Point(GetRoomUser().X + 1, GetRoomUser().Y + 1));
                    }

                    if (Pet.PetData.Energy <= 10)
                    {

                        var moved = false;
                        var items = GetRoom().GetRoomItemHandler().GetWallAndFloor;
                        foreach (
                            var item in
                            items.Where(item => item.GetBaseItem().InteractionType == InteractionType.Petbed))
                        {
                            if (moved) continue;
                            Pet.MoveTo(item.GetX, item.GetY);
                            moved = true;
                            ActionTimer = 30;
                        }
                        if (moved)
                        {
                            Pet.PetData.PetEnergy(true);
                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;
                            ActionTimer = 35;
                        }
                        Pet.Chat("ZZZZZZ", false);
                    }
                    else
                    {
                        var Speech = OblivionServer.GetGame().GetChatManager().GetPetLocale().GetValue("pet.lazy");

                        Pet.Chat(Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)], false);

                        Pet.PetData.PetEnergy(false); // Remove Energy
                    }
                }
            }
            //Pet = null;
        }

        #endregion
    }
}