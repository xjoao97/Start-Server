#region

using System;
using System.Data;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorRoomChatlogComposer : ServerPacket
    {
        public ModeratorRoomChatlogComposer(Room Room)
            : base(ServerPacketHeader.ModeratorRoomChatlogMessageComposer)
        {
            WriteByte(1);
            WriteShort(2); //Count
            WriteString("roomName");
            WriteByte(2);
            WriteString(Room.Name);
            WriteString("roomId");
            WriteByte(1);
            WriteInteger(Room.Id);

            DataTable Table;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT id,user_id,room_id, message,DATE_FORMAT(from_unixtime(timestamp),'%k:%i') FROM `chatlogs` WHERE `room_id` = @rid ORDER BY `id` DESC LIMIT 250");
                dbClient.AddParameter("rid", Room.Id);
                Table = dbClient.GetTable();
            }

            WriteShort(Table.Rows.Count);
            foreach (DataRow Row in Table.Rows)
            {
                var Habbo = OblivionServer.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(Row["user_id"]));
                //FALTA ARREGLAR QUE SE VEA LA HORA
                if (Habbo == null)
                {
                    WriteString(Convert.ToString(Row[4]));
                    WriteInteger(-1);
                    WriteString("Unknown User");
                    WriteString(string.IsNullOrWhiteSpace(Convert.ToString(Row["message"]))
                        ? "*user sent a blank message*"
                        : Convert.ToString(Row["message"]));
                    WriteBoolean(false);
                }
                else
                {
                    WriteString(Convert.ToString(Row[4]));
                    WriteInteger(Habbo.Id);
                    WriteString(Habbo.Username);
                    WriteString(string.IsNullOrWhiteSpace(Convert.ToString(Row["message"]))
                        ? "*user sent a blank message*"
                        : Convert.ToString(Row["message"]));
                    WriteBoolean(false);
                }
            }
        }
    }
}