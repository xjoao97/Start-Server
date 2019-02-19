﻿#region

using System;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.AI;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UsersComposer : ServerPacket
    {
        public UsersComposer(RoomUser User)
            : base(ServerPacketHeader.UsersMessageComposer)
        {
                WriteInteger(1); //1 avatar
                WriteUser(User);
        }

        private void WriteUser(RoomUser User)
        {
            if (!User.IsPet && !User.IsBot)
            {
                var Habbo = User.GetClient().GetHabbo();

                Group Group = OblivionServer.GetGame()
                                .GetGroupManager()
                                .TryGetGroup(Habbo.GetStats().FavouriteGroupId);

                if (Habbo.PetId == 0)
                {
                    WriteInteger(Habbo.Id);
                    WriteString(Habbo.Username);
                    WriteString(Habbo.Motto);
                    WriteString(Habbo.Look);
                    WriteInteger(User.VirtualId);
                    WriteInteger(User.X);
                    WriteInteger(User.Y);
                    WriteDouble(User.Z);

                    WriteInteger(0); //2 for user, 4 for bot.
                    WriteInteger(1); //1 for user, 2 for pet, 3 for bot.
                    WriteString(Habbo.Gender.ToLower());

                    if (Group != null)
                    {
                        WriteInteger(Group.Id);
                        WriteInteger(0);
                        WriteString(Group.Name);
                    }
                    else
                    {
                        WriteInteger(0);
                        WriteInteger(0);
                        WriteString("");
                    }

                    WriteString(""); //Whats this?
                    WriteInteger(Habbo.GetStats().AchievementPoints); //Achievement score
                    WriteBoolean(false); //Builders club?
                }
                else if (Habbo.PetId > 0 && Habbo.PetId != 100)
                {
                    WriteInteger(Habbo.Id);
                    WriteString(Habbo.Username);
                    WriteString(Habbo.Motto);
                    WriteString(PetFigureForType(Habbo.PetId));

                    WriteInteger(User.VirtualId);
                    WriteInteger(User.X);
                    WriteInteger(User.Y);
                    WriteDouble(User.Z);
                    WriteInteger(0);
                    WriteInteger(2); //Pet.

                    WriteInteger(Habbo.PetId); //pet type.
                    WriteInteger(Habbo.Id); //UserId of the owner.
                    WriteString(Habbo.Username); //Username of the owner.
                    WriteInteger(1);
                    WriteBoolean(false); //Has saddle.
                    WriteBoolean(false); //Is someone riding this horse?
                    WriteInteger(0);
                    WriteInteger(0);
                    WriteString("");
                }
                else if (Habbo.PetId > 0 && Habbo.PetId == 100)
                {
                    WriteInteger(Habbo.Id);
                    WriteString(Habbo.Username);
                    WriteString(Habbo.Motto);
                    WriteString(Habbo.Look.ToLower());
                    WriteInteger(User.VirtualId);
                    WriteInteger(User.X);
                    WriteInteger(User.Y);
                    WriteDouble(User.Z);
                    WriteInteger(0);
                    WriteInteger(4);

                    WriteString(Habbo.Gender.ToLower()); // ?
                    WriteInteger(Habbo.Id); //Owner Id
                    WriteString(Habbo.Username); // Owner name
                    WriteInteger(0); //Action Count
                }
            }
            else if (User.IsPet)
            {
                WriteInteger(User.BotAI.BaseId);
                WriteString(User.BotData.Name);
                WriteString(User.BotData.Motto);

                //base.WriteString("26 30 ffffff 5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24");
                WriteString(User.BotData.Look.ToLower() +
                            (User.PetData.Saddle > 0
                                ? " 3 2 " + User.PetData.PetHair + " " + User.PetData.HairDye + " 3 " +
                                  User.PetData.PetHair + " " + User.PetData.HairDye + " 4 " + User.PetData.Saddle + " 0"
                                : " 2 2 " + User.PetData.PetHair + " " + User.PetData.HairDye + " 3 " +
                                  User.PetData.PetHair + " " + User.PetData.HairDye + ""));

                WriteInteger(User.VirtualId);
                WriteInteger(User.X);
                WriteInteger(User.Y);
                WriteDouble(User.Z);
                WriteInteger(0);
                WriteInteger(User.BotData.AiType == BotAIType.PET ? 2 : 4);
                WriteInteger(User.PetData.Type);
                WriteInteger(User.PetData.OwnerId); // userid
                WriteString(User.PetData.OwnerName); // username
                WriteInteger(1);
                WriteBoolean(User.PetData.Saddle > 0);
                WriteBoolean(User.RidingHorse);
                WriteInteger(0);
                WriteInteger(0);
                WriteString("");
            }
            else if (User.IsBot)
            {
                WriteInteger(User.BotAI.BaseId);
                WriteString(User.BotData.Name);
                WriteString(User.BotData.Motto);
                WriteString(User.BotData.Look.ToLower());
                WriteInteger(User.VirtualId);
                WriteInteger(User.X);
                WriteInteger(User.Y);
                WriteDouble(User.Z);
                WriteInteger(0);
                WriteInteger(User.BotData.AiType == BotAIType.PET ? 2 : 4);

                WriteString(User.BotData.Gender.ToLower()); // ?
                WriteInteger(User.BotData.ownerID); //Owner Id
                WriteString(OblivionServer.GetUsernameById(User.BotData.ownerID)); // Owner name
                WriteInteger(5); //Action Count
                WriteShort(1); //Copy looks
                WriteShort(2); //Setup speech
                WriteShort(3); //Relax
                WriteShort(4); //Dance
                WriteShort(5); //Change name
            }
        }

        public string PetFigureForType(int Type)
        {
            var _random = new Random();

            switch (Type)
            {
                    #region Dog Figures

                default:
                case 60:
                {
                    var RandomNumber = _random.Next(1, 4);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "0 0 f08b90 2 2 -1 1 3 -1 1";
                        case 2:
                            return "0 15 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "0 20 d98961 2 2 -1 0 3 -1 0";
                        case 4:
                            return "0 21 da9dbd 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Cat Figures.

                case 1:
                {
                    var RandomNumber = _random.Next(1, 5);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "1 18 d5b35f 2 2 -1 0 3 -1 0";
                        case 2:
                            return "1 0 ff7b3a 2 2 -1 0 3 -1 0";
                        case 3:
                            return "1 18 d98961 2 2 -1 0 3 -1 0";
                        case 4:
                            return "1 0 ff7b3a 2 2 -1 0 3 -1 1";
                        case 5:
                            return "1 24 d5b35f 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Terrier Figures

                case 2:
                {
                    var RandomNumber = _random.Next(1, 6);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "3 3 eeeeee 2 2 -1 0 3 -1 0";
                        case 2:
                            return "3 0 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "3 5 eeeeee 2 2 -1 0 3 -1 0";
                        case 4:
                            return "3 6 eeeeee 2 2 -1 0 3 -1 0";
                        case 5:
                            return "3 4 dddddd 2 2 -1 0 3 -1 0";
                        case 6:
                            return "3 5 dddddd 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Croco Figures

                case 3:
                {
                    var RandomNumber = _random.Next(1, 5);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "2 10 84ce84 2 2 -1 0 3 -1 0";
                        case 2:
                            return "2 8 838851 2 2 0 0 3 -1 0";
                        case 3:
                            return "2 11 b99105 2 2 -1 0 3 -1 0";
                        case 4:
                            return "2 3 e8ce25 2 2 -1 0 3 -1 0";
                        case 5:
                            return "2 2 fcfad3 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Bear Figures

                case 4:
                {
                    var RandomNumber = _random.Next(1, 4);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "4 2 e4feff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "4 3 e4feff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "4 1 eaeddf 2 2 -1 0 3 -1 0";
                        case 4:
                            return "4 0 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Pig Figures

                case 5:
                {
                    var RandomNumber = _random.Next(1, 7);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "5 2 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "5 0 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "5 3 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "5 5 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "5 7 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "5 1 ffffff 2 2 -1 0 3 -1 0";
                        case 7:
                            return "5 8 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Lion Figures

                case 6:
                {
                    var RandomNumber = _random.Next(1, 11);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "6 0 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "6 1 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "6 2 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "6 3 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "6 4 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "6 0 ffd8c9 2 2 -1 0 3 -1 0";
                        case 7:
                            return "6 5 ffffff 2 2 -1 0 3 -1 0";
                        case 8:
                            return "6 11 ffffff 2 2 -1 0 3 -1 0";
                        case 9:
                            return "6 2 ffe49d 2 2 -1 0 3 -1 0";
                        case 10:
                            return "6 11 ff9ae 2 2 -1 0 3 -1 0";
                        case 11:
                            return "6 2 ff9ae 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Rhino Figures

                case 7:
                {
                    var RandomNumber = _random.Next(1, 7);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "7 5 aeaeae 2 2 -1 0 3 -1 0";
                        case 2:
                            return "7 7 ffc99a 2 2 -1 0 3 -1 0";
                        case 3:
                            return "7 5 cccccc 2 2 -1 0 3 -1 0";
                        case 4:
                            return "7 5 9adcff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "7 5 ff7d6a 2 2 -1 0 3 -1 0";
                        case 6:
                            return "7 6 cccccc 2 2 -1 0 3 -1 0";
                        case 7:
                            return "7 0 cccccc 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Spider Figures

                case 8:
                {
                    var RandomNumber = _random.Next(1, 13);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "8 0 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "8 1 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "8 2 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "8 3 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "8 4 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "8 14 ffffff 2 2 -1 0 3 -1 0";
                        case 7:
                            return "8 11 ffffff 2 2 -1 0 3 -1 0";
                        case 8:
                            return "8 8 ffffff 2 2 -1 0 3 -1 0";
                        case 9:
                            return "8 6 ffffff 2 2 -1 0 3 -1 0";
                        case 10:
                            return "8 5 ffffff 2 2 -1 0 3 -1 0";
                        case 11:
                            return "8 9 ffffff 2 2 -1 0 3 -1 0";
                        case 12:
                            return "8 10 ffffff 2 2 -1 0 3 -1 0";
                        case 13:
                            return "8 7 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Turtle Figures

                case 9:
                {
                    var RandomNumber = _random.Next(1, 9);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "9 0 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "9 1 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "9 2 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "9 3 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "9 4 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "9 5 ffffff 2 2 -1 0 3 -1 0";
                        case 7:
                            return "9 6 ffffff 2 2 -1 0 3 -1 0";
                        case 8:
                            return "9 7 ffffff 2 2 -1 0 3 -1 0";
                        case 9:
                            return "9 8 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Chick Figures

                case 10:
                {
                    var RandomNumber = _random.Next(1, 1);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "10 0 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Frog Figures

                case 11:
                {
                    var RandomNumber = _random.Next(1, 13);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "11 1 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "11 2 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "11 3 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "11 4 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "11 5 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "11 9 ffffff 2 2 -1 0 3 -1 0";
                        case 7:
                            return "11 10 ffffff 2 2 -1 0 3 -1 0";
                        case 8:
                            return "11 6 ffffff 2 2 -1 0 3 -1 0";
                        case 9:
                            return "11 12 ffffff 2 2 -1 0 3 -1 0";
                        case 10:
                            return "11 11 ffffff 2 2 -1 0 3 -1 0";
                        case 11:
                            return "11 15 ffffff 2 2 -1 0 3 -1 0";
                        case 12:
                            return "11 13 ffffff 2 2 -1 0 3 -1 0";
                        case 13:
                            return "11 18 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Dragon Figures

                case 12:
                {
                    var RandomNumber = _random.Next(1, 6);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "12 0 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "12 1 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "12 2 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "12 3 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "12 4 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "12 5 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Monkey Figures

                case 14:
                {
                    var RandomNumber = _random.Next(1, 14);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "14 0 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "14 1 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "14 2 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "14 3 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "14 6 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "14 4 ffffff 2 2 -1 0 3 -1 0";
                        case 7:
                            return "14 5 ffffff 2 2 -1 0 3 -1 0";
                        case 8:
                            return "14 7 ffffff 2 2 -1 0 3 -1 0";
                        case 9:
                            return "14 8 ffffff 2 2 -1 0 3 -1 0";
                        case 10:
                            return "14 9 ffffff 2 2 -1 0 3 -1 0";
                        case 11:
                            return "14 10 ffffff 2 2 -1 0 3 -1 0";
                        case 12:
                            return "14 11 ffffff 2 2 -1 0 3 -1 0";
                        case 13:
                            return "14 12 ffffff 2 2 -1 0 3 -1 0";
                        case 14:
                            return "14 13 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Horse Figures

                case 15:
                {
                    var RandomNumber = _random.Next(1, 20);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "15 2 ffffff 2 2 -1 0 3 -1 0";
                        case 2:
                            return "15 3 ffffff 2 2 -1 0 3 -1 0";
                        case 3:
                            return "15 4 ffffff 2 2 -1 0 3 -1 0";
                        case 4:
                            return "15 5 ffffff 2 2 -1 0 3 -1 0";
                        case 5:
                            return "15 6 ffffff 2 2 -1 0 3 -1 0";
                        case 6:
                            return "15 7 ffffff 2 2 -1 0 3 -1 0";
                        case 7:
                            return "15 8 ffffff 2 2 -1 0 3 -1 0";
                        case 8:
                            return "15 9 ffffff 2 2 -1 0 3 -1 0";
                        case 9:
                            return "15 10 ffffff 2 2 -1 0 3 -1 0";
                        case 10:
                            return "15 11 ffffff 2 2 -1 0 3 -1 0";
                        case 11:
                            return "15 12 ffffff 2 2 -1 0 3 -1 0";
                        case 12:
                            return "15 13 ffffff 2 2 -1 0 3 -1 0";
                        case 13:
                            return "15 14 ffffff 2 2 -1 0 3 -1 0";
                        case 14:
                            return "15 15 ffffff 2 2 -1 0 3 -1 0";
                        case 15:
                            return "15 16 ffffff 2 2 -1 0 3 -1 0";
                        case 16:
                            return "15 17 ffffff 2 2 -1 0 3 -1 0";
                        case 17:
                            return "15 78 ffffff 2 2 -1 0 3 -1 0";
                        case 18:
                            return "15 77 ffffff 2 2 -1 0 3 -1 0";
                        case 19:
                            return "15 79 ffffff 2 2 -1 0 3 -1 0";
                        case 20:
                            return "15 80 ffffff 2 2 -1 0 3 -1 0";
                    }
                }

                    #endregion

                    #region Bunny Figures

                case 17:
                {
                    var RandomNumber = _random.Next(1, 8);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "17 1 ffffff";
                        case 2:
                            return "17 2 ffffff";
                        case 3:
                            return "17 3 ffffff";
                        case 4:
                            return "17 4 ffffff";
                        case 5:
                            return "17 5 ffffff";
                        case 6:
                            return "18 0 ffffff";
                        case 7:
                            return "19 0 ffffff";
                        case 8:
                            return "20 0 ffffff";
                    }
                }

                    #endregion

                    #region Pigeon Figures (White & Black)

                case 21:
                {
                    var RandomNumber = _random.Next(1, 3);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "21 0 ffffff";
                        case 2:
                            return "22 0 ffffff";
                    }
                }

                    #endregion

                    #region Demon Monkey Figures

                case 23:
                {
                    var RandomNumber = _random.Next(1, 3);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "23 0 ffffff";
                        case 2:
                            return "23 1 ffffff";
                        case 3:
                            return "23 3 ffffff";
                    }
                }

                    #endregion

                    #region Gnome Figures

                case 26:
                {
                    var RandomNumber = _random.Next(1, 4);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "26 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                        case 2:
                            return "26 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                        case 3:
                            return "26 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                        case 4:
                            return "26 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                    }
                }

                    #endregion

                case 34:
                {
                    var RandomNumber = _random.Next(1, 4);
                    switch (RandomNumber)
                    {
                        default:
                        case 1:
                            return "34 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                        case 2:
                            return "34 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                        case 3:
                            return "34 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                        case 4:
                            return "34 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                    }
                }
                case 35:
                {
                    return "35 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 36:
                {
                    return "36 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 37:
                {
                    return "37 0 ffffff";
                }
                case 38:
                {
                    return "38 3 ffffff";
                }
                case 39:
                {
                    return "39 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 40:
                {
                    return "40 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 41:
                {
                    return "41 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 42:
                {
                    return "42 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 43:
                {
                    return "43 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 44:
                {
                    return "44 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 45:
                {
                    return "45 0 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                }
                case 46:
                    {
                        var RandomNumber = _random.Next(1, 4);
                        switch (RandomNumber)
                        {
                            default:
                            case 1:
                                return "46 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                            case 2:
                                return "46 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                            case 3:
                                return "46 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                            case 4:
                                return "46 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                        }
                    }

                case 47:
                    {
                        var RandomNumber = _random.Next(1, 4);
                        switch (RandomNumber)
                        {
                            default:
                            case 1:
                                return "47 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3";
                            case 2:
                                return "47 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3";
                            case 3:
                                return "47 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6";
                            case 4:
                                return "47 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3";
                        }
                    }
            }
        }
    }
}