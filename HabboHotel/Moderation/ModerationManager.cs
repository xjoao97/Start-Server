#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Oblivion.HabboHotel.Support;

#endregion

namespace Oblivion.HabboHotel.Moderation
{
    public sealed class ModerationManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.HabboHotel.Moderation.ModerationManager");
        private readonly Dictionary<string, ModerationBan> _bans = new Dictionary<string, ModerationBan>();

        private readonly ModerationTool _moderationTool;

        private readonly List<string> _roomPresets = new List<string>();

        private readonly Dictionary<int, string> _userActionPresetCategories = new Dictionary<int, string>();

        private readonly Dictionary<int, List<ModerationPresetActionMessages>> _userActionPresetMessages =
            new Dictionary<int, List<ModerationPresetActionMessages>>();

        private readonly List<string> _userPresets = new List<string>();

        public ModerationManager()
        {
            _moderationTool = new ModerationTool();

            Init();

            Log.Info("Moderation Manager -> LOADED");
        }

        public ICollection<string> UserMessagePresets => _userPresets;

        public ICollection<string> RoomMessagePresets => _roomPresets;

        public Dictionary<string, List<ModerationPresetActionMessages>> UserActionPresets
        {
            get
            {
                var result = new Dictionary<string, List<ModerationPresetActionMessages>>();
                foreach (var category in _userActionPresetCategories.ToList())
                {
                    result.Add(category.Value, new List<ModerationPresetActionMessages>());

                    if (!_userActionPresetMessages.ContainsKey(category.Key)) continue;
                    foreach (var data in _userActionPresetMessages[category.Key])
                        result[category.Value].Add(data);
                }
                return result;
            }
        }

        public ModerationTool GetModerationTool() => _moderationTool;


        public void Init()
        {
            _userPresets.Clear();
            _userActionPresetCategories.Clear();
            _userActionPresetMessages.Clear();
            _bans.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `moderation_presets`;");
                var presetsTable = dbClient.getTable();

                if (presetsTable != null)
                    foreach (DataRow row in presetsTable.Rows)
                    {
                        var type = Convert.ToString(row["type"]).ToLower();
                        switch (type)
                        {
                            case "user":
                                _userPresets.Add(Convert.ToString(row["message"]));
                                break;

                            case "room":
                                _roomPresets.Add(Convert.ToString(row["message"]));
                                break;
                        }
                    }
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_categories`;");
                var presetsActionCats = dbClient.getTable();

                if (presetsActionCats != null)
                    foreach (DataRow row in presetsActionCats.Rows)
                        _userActionPresetCategories.Add(Convert.ToInt32(row["id"]), Convert.ToString(row["caption"]));
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_messages`;");
                var presetsActionMessages = dbClient.getTable();

                if (presetsActionMessages != null)
                    foreach (DataRow row in presetsActionMessages.Rows)
                    {
                        var parentId = Convert.ToInt32(row["parent_id"]);

                        if (!_userActionPresetMessages.ContainsKey(parentId))
                            _userActionPresetMessages.Add(parentId, new List<ModerationPresetActionMessages>());

                        _userActionPresetMessages[parentId].Add(
                            new ModerationPresetActionMessages(Convert.ToInt32(row["id"]),
                                Convert.ToInt32(row["parent_id"]), Convert.ToString(row["caption"]),
                                Convert.ToString(row["message_text"]),
                                Convert.ToInt32(row["mute_hours"]), Convert.ToInt32(row["ban_hours"]),
                                Convert.ToInt32(row["ip_ban_hours"]), Convert.ToInt32(row["trade_lock_days"]),
                                Convert.ToString(row["notice"])));
                    }
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                var getBans = dbClient.getTable();

                if (getBans != null)
                    foreach (DataRow dRow in getBans.Rows)
                    {
                        var value = Convert.ToString(dRow["value"]);
                        var reason = Convert.ToString(dRow["reason"]);
                        var expires = (double) dRow["expire"];
                        var type = Convert.ToString(dRow["bantype"]);

                        var ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (expires > OblivionServer.GetUnixTimestamp())
                        {
                            if (!_bans.ContainsKey(value))
                                _bans.Add(value, ban);
                        }
                        else
                        {
                            dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" +
                                              BanTypeUtility.FromModerationBanType(ban.Type) +
                                              "' AND `value` = @Key LIMIT 1");
                            dbClient.AddParameter("Key", value);
                            dbClient.RunQuery();
                        }
                    }
            }

            Log.Info("Loaded " + (_userPresets.Count + _roomPresets.Count) + " moderation presets.");
            Log.Info("Loaded " + _userActionPresetCategories.Count + " moderation categories.");
            Log.Info("Loaded " + _userActionPresetMessages.Count + " moderation action preset messages.");
            Log.Info("Cached " + _bans.Count + " username and machine bans.");
        }

        public void ReCacheBans()
        {
            if (_bans.Count > 0)
                _bans.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                var getBans = dbClient.getTable();

                if (getBans != null)
                    foreach (DataRow dRow in getBans.Rows)
                    {
                        var value = Convert.ToString(dRow["value"]);
                        var reason = Convert.ToString(dRow["reason"]);
                        var expires = (double) dRow["expire"];
                        var type = Convert.ToString(dRow["bantype"]);

                        var ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (expires > OblivionServer.GetUnixTimestamp())
                        {
                            if (!_bans.ContainsKey(value))
                                _bans.Add(value, ban);
                        }
                        else
                        {
                            dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" +
                                              BanTypeUtility.FromModerationBanType(ban.Type) +
                                              "' AND `value` = @Key LIMIT 1");
                            dbClient.AddParameter("Key", value);
                            dbClient.RunQuery();
                        }
                    }
            }

            Log.Info("Cached " + _bans.Count + " username and machine bans.");
        }

        public void BanUser(string mod, ModerationBanType type, string banValue, string reason, double expireTimestamp)
        {
            var banType = type == ModerationBanType.IP ? "ip" : type == ModerationBanType.MACHINE ? "machine" : "user";
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "REPLACE INTO `bans` (`bantype`, `value`, `reason`, `expire`, `added_by`,`added_date`) VALUES ('" +
                    banType + "', @value, @ReasoN, " + expireTimestamp + ", '" + mod + "', '" +
                    OblivionServer.GetUnixTimestamp() + "');");
                dbClient.AddParameter("reason", reason);
                dbClient.AddParameter("value", banValue);
                dbClient.RunQuery();
            }

            if (type != ModerationBanType.MACHINE && type != ModerationBanType.USERNAME) return;
            if (!_bans.ContainsKey(banValue))
                _bans.Add(banValue, new ModerationBan(type, banValue, reason, expireTimestamp));
        }

        /// <summary>
        ///     Runs a quick check to see if a ban record is cached in the server.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ban"></param>
        /// <returns></returns>
        public bool IsBanned(string key, out ModerationBan ban)
        {
            if (!_bans.TryGetValue(key, out ban)) return false;
            if (!ban.Expired)
                return true;

            //This ban has expired, let us quickly remove it here.
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" +
                                  BanTypeUtility.FromModerationBanType(ban.Type) + "' AND `value` = @Key LIMIT 1");
                dbClient.AddParameter("Key", key);
                dbClient.RunQuery();
            }

            //And finally, let us remove the ban record from the cache.
            if (_bans.ContainsKey(key))
                _bans.Remove(key);
            return false;
        }

        /// <summary>
        ///     Run a quick database check to see if this ban exists in the database.
        /// </summary>
        /// <param name="machineId">The value of the ban.</param>
        /// <returns></returns>
        public bool MachineBanCheck(string machineId)
        {
            ModerationBan machineBanRecord;
            if (!OblivionServer.GetGame().GetModerationManager().IsBanned(machineId, out machineBanRecord)) return true;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'machine' AND `value` = @value LIMIT 1");
                dbClient.AddParameter("value", machineId);
                var banRow = dbClient.getRow();

                //If there is no more ban record, then we can simply remove it from our cache!
                if (banRow != null) return true;
                OblivionServer.GetGame().GetModerationManager().RemoveBan(machineId);
                return false;
            }
        }

        /// <summary>
        ///     Run a quick database check to see if this ban exists in the database.
        /// </summary>
        /// <param name="username">The value of the ban.</param>
        /// <returns></returns>
        public bool UsernameBanCheck(string username)
        {
            ModerationBan usernameBanRecord;
            if (!OblivionServer.GetGame().GetModerationManager().IsBanned(username, out usernameBanRecord)) return true;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'user' AND `value` = @value LIMIT 1");
                dbClient.AddParameter("value", username);
                var banRow = dbClient.getRow();

                //If there is no more ban record, then we can simply remove it from our cache!
                if (banRow != null) return true;
                OblivionServer.GetGame().GetModerationManager().RemoveBan(username);
                return false;
            }
        }

        /// <summary>
        ///     Remove a ban from the cache based on a given value.
        /// </summary>
        /// <param name="value"></param>
        public void RemoveBan(string value) => _bans.Remove(value);
    }
}