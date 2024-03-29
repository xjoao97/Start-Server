﻿#region

using System;
using System.Data;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Avatar
{
    internal class WardrobeComposer : ServerPacket
    {
        public WardrobeComposer(GameClient Session)
            : base(ServerPacketHeader.WardrobeMessageComposer)
        {
            WriteInteger(1);
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `slot_id`,`look`,`gender` FROM `user_wardrobe` WHERE `user_id` = '" +
                                  Session.GetHabbo().Id + "'");
                var WardrobeData = dbClient.GetTable();

                if (WardrobeData == null)
                {
                    WriteInteger(0);
                }
                else
                {
                    WriteInteger(WardrobeData.Rows.Count);
                    foreach (DataRow Row in WardrobeData.Rows)
                    {
                        WriteInteger(Convert.ToInt32(Row["slot_id"]));
                        WriteString(Convert.ToString(Row["look"]));
                        WriteString(Row["gender"].ToString().ToUpper());
                    }
                }
            }
        }
    }
}