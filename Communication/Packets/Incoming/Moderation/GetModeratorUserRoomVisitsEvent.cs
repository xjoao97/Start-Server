#region

using System;
using System.Collections.Generic;
using System.Data;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorUserRoomVisitsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            var UserId = Packet.PopInt();
            var Target = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Target == null)
                return;

            var Visits = new Dictionary<double, RoomData>();
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT room_id, `entry_timestamp` FROM `user_roomvisits` WHERE `user_id` =@id ORDER BY `entry_timestamp` DESC LIMIT 50");
                dbClient.AddParameter("id", UserId);
                var Table = dbClient.GetTable();

                if (Table != null)
                    foreach (DataRow Row in Table.Rows)
                    {
                        var RData =
                            OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Convert.ToInt32(Row["room_id"]));
                        if (RData == null)
                            return;

                        if (!Visits.ContainsKey(Convert.ToDouble(Row["entry_timestamp"])))
                            Visits.Add(Convert.ToDouble(Row["entry_timestamp"]), RData);
                    }
            }

            Session.SendMessage(new ModeratorUserRoomVisitsComposer(Target.GetHabbo(), Visits));
        }
    }
}