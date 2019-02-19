#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Items.Data.Toner;

#endregion

namespace Oblivion.HabboHotel.Items
{
    internal static class ItemBehaviourUtility
    {
        public static void GenerateExtradata(Item Item, ServerPacket Message)
        {
            if (Item == null || Message == null)
                return;

            switch (Item.GetBaseItem().InteractionType)
            {
                default:
                    Message.WriteInteger(1);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.GetBaseItem().InteractionType != InteractionType.FootballGate
                        ? Item.ExtraData
                        : string.Empty);
                    break;
                case InteractionType.WiredScoreBoard:
                    var name = Item.GetBaseItem().ItemName;
                    var type = name.Split('*')[1];

                    if (type != null)
                    {
                        var ScoreBordata = new Dictionary<int, KeyValuePair<int, string>>();
                        Message.WriteInteger(0);
                        Message.WriteInteger(6);
                        Message.WriteString("1");
                        Message.WriteInteger(1);


                        if (Item.GetRoom() != null)
                        {
                            switch (type)
                            {
                                case "2":
                                    Message.WriteInteger(1);
                                    Message.WriteInteger(Item.GetRoom().WiredScoreBordDay.Count);
                                    ScoreBordata = Item.GetRoom().WiredScoreBordDay;
                                    break;

                                case "3":
                                    Message.WriteInteger(2);
                                    Message.WriteInteger(Item.GetRoom().WiredScoreBordWeek.Count);
                                    ScoreBordata = Item.GetRoom().WiredScoreBordWeek;
                                    break;


                                case "4":
                                    Message.WriteInteger(3);
                                    Message.WriteInteger(Item.GetRoom().WiredScoreBordMonth.Count);
                                    ScoreBordata = Item.GetRoom().WiredScoreBordMonth;
                                    break;

                                default:
                                    Message.WriteInteger(1);
                                    Message.WriteInteger(0);
                                    ScoreBordata = null;
                                    break;
                            }
                        }
                        else
                        {
                            Message.WriteInteger(1);
                            Message.WriteInteger(1);
                            Message.WriteInteger(0);
                            Message.WriteInteger(1);
                            Message.WriteString("Dit Scorebord werkt nog niet :(");
                        }

                        if (ScoreBordata?.Count != 0)
                            foreach (var value in (
                                from i in ScoreBordata
                                orderby i.Value.Key descending
                                select i).ToDictionary(i => i.Key, i => i.Value).Values)
                            {
                                var username = value.Value;
                                Message.WriteInteger(value.Key);
                                Message.WriteInteger(1);
                                Message.WriteString(string.IsNullOrEmpty(username) ? string.Empty : username);
                            }
                    }
                    break;
                case InteractionType.GnomeBox:
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString("");
                    break;

                case InteractionType.PetBreedingBox:
                case InteractionType.PurchasableClothing:
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString("0");
                    break;

                case InteractionType.Stacktool:
                    Message.WriteInteger(0);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData);
                    break;

                case InteractionType.Wallpaper:
                    Message.WriteInteger(2);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData);

                    break;
                case InteractionType.Floor:
                    Message.WriteInteger(3);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData);
                    break;

                case InteractionType.Landscape:
                    Message.WriteInteger(4);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData);
                    break;
                case InteractionType.MusicDisc:
                    int musicid;
                    int.TryParse(Item.ExtraData, out musicid);
                    Message.WriteInteger(musicid);
                    Message.WriteInteger(0);
                    Message.WriteString(Item.ExtraData);
                    break;

                case InteractionType.GuildItem:
                case InteractionType.GuildGate:
                case InteractionType.GuildForum:
                    Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(Item.GroupId);
                    if (Group == null)
                    {
                        Message.WriteInteger(1);
                        Message.WriteInteger(0);
                        Message.WriteString(Item.ExtraData);
                    }
                    else
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(2);
                        Message.WriteInteger(5);
                        Message.WriteString(Item.ExtraData);
                        Message.WriteString(Group.Id.ToString());
                        Message.WriteString(Group.Badge);
                        Message.WriteString(OblivionServer.GetGame()
                            .GetGroupManager()
                            .GetGroupColour(Group.Colour1, true));
                        // Group Colour 1
                        Message.WriteString(OblivionServer.GetGame()
                            .GetGroupManager()
                            .GetGroupColour(Group.Colour2, false));
                        // Group Colour 2
                    }
                    break;

                case InteractionType.Background:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    if (!string.IsNullOrEmpty(Item.ExtraData))
                    {
                        Message.WriteInteger(Item.ExtraData.Split(Convert.ToChar(9)).Length / 2);

                        for (var i = 0; i <= Item.ExtraData.Split(Convert.ToChar(9)).Length - 1; i++)
                            Message.WriteString(Item.ExtraData.Split(Convert.ToChar(9))[i]);
                    }
                    else
                    {
                        Message.WriteInteger(0);
                    }
                    break;

                case InteractionType.Gift:
                {
                    var ExtraData = Item.ExtraData.Split(Convert.ToChar(5));
                    if (ExtraData.Length != 7)
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteString(Item.ExtraData);
                    }
                    else
                    {
                        var Style = int.Parse(ExtraData[6]) * 1000 + int.Parse(ExtraData[6]);

                        var Purchaser =
                            OblivionServer.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(ExtraData[2]));
                        if (Purchaser == null)
                        {
                            Message.WriteInteger(0);
                            Message.WriteInteger(0);
                            Message.WriteString(Item.ExtraData);
                        }
                        else
                        {
                            Message.WriteInteger(Style);
                            Message.WriteInteger(1);
                            Message.WriteInteger(6);
                            Message.WriteString("EXTRA_PARAM");
                            Message.WriteString("");
                            Message.WriteString("MESSAGE");
                            Message.WriteString(ExtraData[1]);
                            Message.WriteString("PURCHASER_NAME");
                            Message.WriteString(Purchaser.Username);
                            Message.WriteString("PURCHASER_FIGURE");
                            Message.WriteString(Purchaser.Look);
                            Message.WriteString("PRODUCT_CODE");
                            Message.WriteString("A1 KUMIANKKA");
                            Message.WriteString("state");
                            Message.WriteString(Item.MagicRemove ? "1" : "0");
                        }
                    }
                }
                    break;

                case InteractionType.Mannequin:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    Message.WriteInteger(3);
                    if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        var Stuff = Item.ExtraData.Split(Convert.ToChar(5));
                        Message.WriteString("GENDER");
                        Message.WriteString(Stuff[0]);
                        Message.WriteString("FIGURE");
                        Message.WriteString(Stuff[1]);
                        Message.WriteString("OUTFIT_NAME");
                        Message.WriteString(Stuff[2]);
                    }
                    else
                    {
                        Message.WriteString("GENDER");
                        Message.WriteString("");
                        Message.WriteString("FIGURE");
                        Message.WriteString("");
                        Message.WriteString("OUTFIT_NAME");
                        Message.WriteString("");
                    }
                    break;

                case InteractionType.Toner:
                    if (Item.RoomId != 0)
                    {
                        if (Item.GetRoom().TonerData == null)
                            Item.GetRoom().TonerData = new TonerData(Item.Id);

                        Message.WriteInteger(0);
                        Message.WriteInteger(5);
                        Message.WriteInteger(4);
                        Message.WriteInteger(Item.GetRoom().TonerData.Enabled);
                        Message.WriteInteger(Item.GetRoom().TonerData.Hue);
                        Message.WriteInteger(Item.GetRoom().TonerData.Saturation);
                        Message.WriteInteger(Item.GetRoom().TonerData.Lightness);
                    }
                    else
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteString(string.Empty);
                    }
                    break;

                case InteractionType.BadgeDisplay:
                    Message.WriteInteger(0);
                    Message.WriteInteger(2);
                    Message.WriteInteger(4);

                    var BadgeData = Item.ExtraData.Split(Convert.ToChar(9));
                    if (Item.ExtraData.Contains(Convert.ToChar(9).ToString()))
                    {
                        Message.WriteString("0"); //No idea
                        Message.WriteString(BadgeData[0]); //Badge name
                        Message.WriteString(BadgeData[1]); //Owner
                        Message.WriteString(BadgeData[2]); //Date
                    }
                    else
                    {
                        Message.WriteString("0"); //No idea
                        Message.WriteString("DEV"); //Badge name
                        Message.WriteString("Sledmore"); //Owner
                        Message.WriteString("13-13-1337"); //Date
                    }
                    break;

                case InteractionType.Television:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    Message.WriteInteger(1);

                    Message.WriteString("THUMBNAIL_URL");
                    //Message.WriteString("http://img.youtube.com/vi/" + OblivionServer.GetGame().GetTelevisionManager().TelevisionList.OrderBy(x => Guid.NewGuid()).FirstOrDefault().YouTubeId + "/3.jpg");
                    Message.WriteString("");
                    break;

                case InteractionType.Lovelock:
                    if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                    {
                        var EData = Item.ExtraData.Split((char) 5);
                        var I = 0;
                        Message.WriteInteger(0);
                        Message.WriteInteger(2);
                        Message.WriteInteger(EData.Length);
                        while (I < EData.Length)
                        {
                            Message.WriteString(EData[I]);
                            I++;
                        }
                    }
                    else
                    {
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteString("0");
                    }
                    break;

                case InteractionType.MonsterplantSeed:
                    Message.WriteInteger(0);
                    Message.WriteInteger(1);
                    Message.WriteInteger(1);

                    Message.WriteString("rarity");
                    Message.WriteString("1"); //Leve should be dynamic.
                    break;
            }
        }

        public static void GenerateWallExtradata(Item Item, ServerPacket Message)
        {
            switch (Item.GetBaseItem().InteractionType)
            {
                default:
                    Message.WriteString(Item.ExtraData);
                    break;

                case InteractionType.Postit:
                    Message.WriteString(Item.ExtraData.Split(' ')[0]);
                    break;
            }
        }
    }
}