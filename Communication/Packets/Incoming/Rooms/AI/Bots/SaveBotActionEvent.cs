#region

using System;
using System.Collections.Generic;
using System.Data;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.AI.Speech;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.AI.Bots
{
    internal class SaveBotActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var BotId = Packet.PopInt();
            var ActionId = Packet.PopInt();
            var DataString = Packet.PopString();

            if (ActionId < 1 || ActionId > 5)
                return;

            RoomUser Bot;
            if (!Room.GetRoomUserManager().TryGetBot(BotId, out Bot))
                return;

            if (Bot.BotData.ownerID != Session.GetHabbo().Id &&
                !Session.GetHabbo().GetPermissions().HasRight("bot_edit_any_override"))
                return;

            var RoomBot = Bot.BotData;
            if (RoomBot == null)
                return;

            /* 1 = Copy looks
             * 2 = Setup Speech
             * 3 = Relax
             * 4 = Dance
             * 5 = Change Name
             */

            switch (ActionId)
            {
                    #region Copy Looks (1)

                case 1:
                {
                    var UserChangeComposer = new ServerPacket(ServerPacketHeader.UserChangeMessageComposer);
                    UserChangeComposer.WriteInteger(Bot.VirtualId);
                    UserChangeComposer.WriteString(Session.GetHabbo().Look);
                    UserChangeComposer.WriteString(Session.GetHabbo().Gender);
                    UserChangeComposer.WriteString(Bot.BotData.Motto);
                    UserChangeComposer.WriteInteger(0);
                    Room.SendMessage(UserChangeComposer);

                    //Change the defaults
                    Bot.BotData.Look = Session.GetHabbo().Look;
                    Bot.BotData.Gender = Session.GetHabbo().Gender;

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `bots` SET `look` = @look, `gender` = '" + Session.GetHabbo().Gender +
                                          "' WHERE `id` = '" + Bot.BotData.Id + "' LIMIT 1");
                        dbClient.AddParameter("look", Session.GetHabbo().Look);
                        dbClient.RunQuery();
                    }

                    //Room.SendMessage(new UserChangeComposer(BotUser.GetClient(), true));
                    break;
                }

                    #endregion

                    #region Setup Speech (2)

                case 2:
                {
                    var ConfigData = DataString.Split(new[]
                    {
                        ";#;"
                    }, StringSplitOptions.None);

                    var SpeechData = ConfigData[0].Split(new[]
                    {
                        '\r',
                        '\n'
                    }, StringSplitOptions.RemoveEmptyEntries);

                    var AutomaticChat = Convert.ToString(ConfigData[1]);
                    var SpeakingInterval = Convert.ToString(ConfigData[2]);
                    var MixChat = Convert.ToString(ConfigData[3]);

                    if (string.IsNullOrEmpty(SpeakingInterval) || Convert.ToInt32(SpeakingInterval) <= 0 ||
                        Convert.ToInt32(SpeakingInterval) < 7)
                        SpeakingInterval = "7";

                    RoomBot.AutomaticChat = Convert.ToBoolean(AutomaticChat);
                    RoomBot.SpeakingInterval = Convert.ToInt32(SpeakingInterval);
                    RoomBot.MixSentences = Convert.ToBoolean(MixChat);

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.runFastQuery("DELETE FROM `bots_speech` WHERE `bot_id` = '" + Bot.BotData.Id + "'");
                    }

                    #region Save Data - TODO: MAKE METHODS FOR THIS.

                    for (var i = 0; i <= SpeechData.Length - 1; i++)
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("INSERT INTO `bots_speech` (`bot_id`, `text`) VALUES (@id, @data)");
                            dbClient.AddParameter("id", BotId);
                            dbClient.AddParameter("data", SpeechData[i]);
                            dbClient.RunQuery();

                            dbClient.SetQuery(
                                "UPDATE `bots` SET `automatic_chat` = @AutomaticChat, `speaking_interval` = @SpeakingInterval, `mix_sentences` = @MixChat WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", BotId);
                            dbClient.AddParameter("AutomaticChat", AutomaticChat.ToLower());
                            dbClient.AddParameter("SpeakingInterval", Convert.ToInt32(SpeakingInterval));
                            dbClient.AddParameter("MixChat", OblivionServer.BoolToEnum(Convert.ToBoolean(MixChat)));
                            dbClient.RunQuery();
                        }

                    #endregion

                    #region Handle Speech

                    RoomBot.RandomSpeech.Clear();
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @id");
                        dbClient.AddParameter("id", BotId);

                        var BotSpeech = dbClient.getTable();

                        var Speeches = new List<RandomSpeech>();
                        foreach (DataRow Speech in BotSpeech.Rows)
                            RoomBot.RandomSpeech.Add(new RandomSpeech(Convert.ToString(Speech["text"]), BotId));
                    }

                    #endregion

                    break;
                }

                    #endregion

                    #region Relax (3)

                case 3:
                {
                    Bot.BotData.WalkingMode = Bot.BotData.WalkingMode == "stand" ? "freeroam" : "stand";

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.runFastQuery("UPDATE `bots` SET `walk_mode` = '" + Bot.BotData.WalkingMode +
                                          "' WHERE `id` = '" + Bot.BotData.Id + "' LIMIT 1");
                    }
                    break;
                }

                    #endregion

                    #region Dance (4)

                case 4:
                {
                    if (Bot.BotData.DanceId > 0)
                    {
                        Bot.BotData.DanceId = 0;
                    }
                    else
                    {
                        var RandomDance = new Random();
                        Bot.BotData.DanceId = RandomDance.Next(1, 4);
                    }

                    Room.SendMessage(new DanceComposer(Bot, Bot.BotData.DanceId));
                    break;
                }

                    #endregion

                    #region Change Name (5)

                case 5:
                {
                    if (DataString.Length == 0)
                    {
                        Session.SendWhisper("Come on, atleast give the bot a name!");
                        return;
                    }
                    if (DataString.Length >= 16)
                    {
                        Session.SendWhisper("Come on, the bot doesn't need a name that long!");
                        return;
                    }

                    if (DataString.Contains("<img src") || DataString.Contains("<font ") ||
                        DataString.Contains("</font>") || DataString.Contains("</a>") || DataString.Contains("<i>"))
                    {
                        Session.SendWhisper("No HTML, please :<");
                        return;
                    }

                    Bot.BotData.Name = DataString;
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `bots` SET `name` = @name WHERE `id` = '" + Bot.BotData.Id +
                                          "' LIMIT 1");
                        dbClient.AddParameter("name", DataString);
                        dbClient.RunQuery();
                    }
                    Room.SendMessage(new UsersComposer(Bot));
                    break;
                }

                    #endregion
            }
        }
    }
}