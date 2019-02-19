#region

using System;
using System.Data;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorUserChatlogComposer : ServerPacket
    {
        public ModeratorUserChatlogComposer(int UserId)
            : base(ServerPacketHeader.ModeratorUserChatlogMessageComposer)
        {
            WriteInteger(UserId);
            WriteString(OblivionServer.GetGame().GetClientManager().GetNameById(UserId));
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT room_id,entry_timestamp,exit_timestamp FROM user_roomvisits WHERE `user_id` = " + UserId +
                    " ORDER BY entry_timestamp DESC LIMIT 5");
                var Visits = dbClient.getTable();

                if (Visits != null)
                {
                    WriteInteger(Visits.Rows.Count);
                    foreach (DataRow Visit in Visits.Rows)
                    {
                        var RoomName = "Unknown";

                        var Room = OblivionServer.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(Visit["room_id"]));

                        if (Room != null)
                            RoomName = Room.Name;

                        WriteByte(1);
                        WriteShort(2); //Count
                        WriteString("roomName");
                        WriteByte(2);
                        WriteString(RoomName); // room name
                        WriteString("roomId");
                        WriteByte(1);
                        WriteInteger(Convert.ToInt32(Visit["room_id"]));

                        DataTable Chatlogs = null;
                        if ((double) Visit["exit_timestamp"] <= 0)
                            Visit["exit_timestamp"] = OblivionServer.GetUnixTimestamp();

                        //dbClient.SetQuery("SELECT user_id,timestamp,message FROM `chatlogs` WHERE room_id = " + Convert.ToInt32(Visit["room_id"]) + " AND timestamp > " + (Double)Visit["entry_timestamp"] + " AND timestamp < " + (Double)Visit["exit_timestamp"] + " ORDER BY timestamp DESC LIMIT 150");
                        dbClient.SetQuery(
                            "SELECT user_id,DATE_FORMAT(from_unixtime(timestamp),'%k:%i'),message FROM `chatlogs` WHERE room_id = " +
                            Convert.ToInt32(Visit["room_id"]) + "  ORDER BY id DESC LIMIT 150");
                        Chatlogs = dbClient.getTable();

                        if (Chatlogs != null)
                        {
                            WriteShort(Chatlogs.Rows.Count);
                            foreach (DataRow Log in Chatlogs.Rows)
                            {
                                var Habbo =
                                    OblivionServer.GetGame()
                                        .GetCacheManager()
                                        .GenerateUser(Convert.ToInt32(Log["user_id"]));

                                if (Habbo == null)
                                    continue;
                                //base.WriteInteger(((int)OblivionServer.GetUnixTimestamp() - Convert.ToInt32(Log["timestamp"])) * 1000);
                                //int Time = ((int)OblivionServer.GetUnixTimestamp() - Convert.ToInt32(Log["timestamp"]) * 1000);
                                //int Time = (int)OblivionServer.GetUnixTimestamp();
                                //DateTime time = new DateTime(Convert.ToInt32(Time));
                                WriteString(Convert.ToString(Log[1]));
                                //base.WriteString(Time.ToString());
                                WriteInteger(Habbo.Id);
                                WriteString(Habbo.Username);
                                WriteString(string.IsNullOrWhiteSpace(Convert.ToString(Log["message"]))
                                    ? "*user sent a blank message*"
                                    : Convert.ToString(Log["message"]));
                                WriteBoolean(false);
                            }
                        }
                        else
                        {
                            WriteInteger(0);
                        }
                    }
                }
                else
                {
                    WriteInteger(0);
                }
            }
        }
    }
}