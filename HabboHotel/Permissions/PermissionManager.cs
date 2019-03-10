#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Permissions
{
    public sealed class PermissionManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Permissions.PermissionManager");

        private readonly Dictionary<string, PermissionCommand> _commands = new Dictionary<string, PermissionCommand>();

        private readonly Dictionary<int, List<string>> PermissionGroupRights = new Dictionary<int, List<string>>();

        private readonly Dictionary<int, PermissionGroup> PermissionGroups = new Dictionary<int, PermissionGroup>();

        private readonly Dictionary<int, Permission> Permissions = new Dictionary<int, Permission>();


        public void Init()
        {
            Permissions.Clear();
            _commands.Clear();
            PermissionGroups.Clear();
            PermissionGroupRights.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions`");
                var GetPermissions = dbClient.GetTable();

                if (GetPermissions != null)
                    foreach (DataRow Row in GetPermissions.Rows)
                        Permissions.Add(Convert.ToInt32(Row["id"]),
                            new Permission(Convert.ToInt32(Row["id"]), Convert.ToString(Row["permission"]),
                                Convert.ToString(Row["description"])));
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_commands`");
                var GetCommands = dbClient.GetTable();

                if (GetCommands != null)
                    foreach (DataRow Row in GetCommands.Rows)
                        _commands.Add(Convert.ToString(Row["command"]),
                            new PermissionCommand(Convert.ToString(Row["command"]), Convert.ToInt32(Row["group_id"])));
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_groups`");
                var GetPermissionGroups = dbClient.GetTable();

                if (GetPermissionGroups != null)
                    foreach (DataRow Row in GetPermissionGroups.Rows)
                        PermissionGroups.Add(Convert.ToInt32(Row["id"]),
                            new PermissionGroup(Convert.ToString("name"), Convert.ToString("description"),
                                Convert.ToString("badge")));
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_rights`");
                var GetPermissionRights = dbClient.GetTable();

                if (GetPermissionRights != null)
                    foreach (DataRow Row in GetPermissionRights.Rows)
                    {
                        var GroupId = Convert.ToInt32(Row["group_id"]);
                        var PermissionId = Convert.ToInt32(Row["permission_id"]);

                        if (!PermissionGroups.ContainsKey(GroupId))
                            continue; // permission group does not exist

                        Permission Permission;

                        if (!Permissions.TryGetValue(PermissionId, out Permission))
                            continue; // permission does not exist

                        if (PermissionGroupRights.ContainsKey(GroupId))
                        {
                            PermissionGroupRights[GroupId].Add(Permission.PermissionName);
                        }
                        else
                        {
                            var RightsSet = new List<string>
                            {
                                Permission.PermissionName
                            };

                            PermissionGroupRights.Add(GroupId, RightsSet);
                        }
                    }
            }


            log.Info("Loaded " + Permissions.Count + " permissions.");
            log.Info("Loaded " + PermissionGroups.Count + " permissions groups.");
            log.Info("Loaded " + PermissionGroupRights.Count + " permissions group rights.");
        }

        public bool TryGetGroup(int Id, out PermissionGroup Group) => PermissionGroups.TryGetValue(Id, out Group);

        public List<string> GetPermissionsForPlayer(Habbo Player)
        {
            var PermissionSet = new List<string>();

            List<string> PermRights;
            if (PermissionGroupRights.TryGetValue(Player.Rank, out PermRights))
                PermissionSet.AddRange(PermRights);


            return PermissionSet;
        }

        public List<string> GetCommandsForPlayer(Habbo Player) => _commands.Where(x => Player.Rank >= x.Value.GroupId)
            .Select(x => x.Key)
            .ToList();
    }
}