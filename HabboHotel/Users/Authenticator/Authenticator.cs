#region

using System;
using System.Data;

#endregion

namespace Oblivion.HabboHotel.Users.Authenticator
{
    public static class HabboFactory
    {
        public static Habbo GenerateHabbo(DataRow Row, DataRow UserInfo)
            => new Habbo(
                Convert.ToInt32(Row["id"]),
                Convert.ToString(Row["username"]),
                Convert.ToInt32(Row["rank"]),
                Convert.ToString(Row["motto"]),
                Convert.ToString(Row["look"]),
                Convert.ToString(Row["gender"]),
                Convert.ToInt32(Row["credits"]),
                Convert.ToInt32(Row["activity_points"]),
                Convert.ToInt32(Row["home_room"]),
                OblivionServer.EnumToBool(Row["block_newfriends"].ToString()),
                Convert.ToInt32(Row["last_online"]),
                OblivionServer.EnumToBool(Row["hide_online"].ToString()),
                OblivionServer.EnumToBool(Row["hide_inroom"].ToString()),
                Convert.ToDouble(Row["account_created"]),
                Convert.ToInt32(Row["vip_points"]),
                Convert.ToInt32(Row["epoints"]),
                Convert.ToString(Row["machine_id"]),
                Row["nux_user"].ToString() == "true",
                Convert.ToString(Row["volume"]),
                OblivionServer.EnumToBool(Row["chat_preference"].ToString()),
                OblivionServer.EnumToBool(Row["focus_preference"].ToString()),
                OblivionServer.EnumToBool(Row["pets_muted"].ToString()),
                OblivionServer.EnumToBool(Row["bots_muted"].ToString()),
                OblivionServer.EnumToBool(Row["advertising_report_blocked"].ToString()),
                Convert.ToDouble(Row["last_change"].ToString()),
                Convert.ToInt32(Row["gotw_points"]),
                OblivionServer.EnumToBool(Convert.ToString(Row["ignore_invites"])),
                Convert.ToDouble(Row["time_muted"]),
                Convert.ToDouble(UserInfo["trading_locked"]),
                OblivionServer.EnumToBool(Row["allow_gifts"].ToString()),
                Convert.ToInt32(Row["friend_bar_state"]),
                OblivionServer.EnumToBool(Row["disable_forced_effects"].ToString()),
                OblivionServer.EnumToBool(Row["allow_mimic"].ToString()),
                Convert.ToString(Row["prefix_name"]), 
                Convert.ToString(Row["prefix_color"]), 
                Convert.ToString(Row["name_color"]));

    }
}