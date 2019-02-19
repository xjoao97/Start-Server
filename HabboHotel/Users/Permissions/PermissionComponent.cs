#region

using System.Collections.Generic;

#endregion

namespace Oblivion.HabboHotel.Users.Permissions
{
    /// <summary>
    ///     Permissions for a specific Player.
    /// </summary>
    public sealed class PermissionComponent
    {
        private readonly List<string> _commands;

        /// <summary>
        ///     Permission rights are stored here.
        /// </summary>
        private readonly List<string> _permissions;

        public PermissionComponent()
        {
            _permissions = new List<string>();
            _commands = new List<string>();
        }

        /// <summary>
        ///     Init the PermissionComponent.
        /// </summary>
        /// <param name="Player"></param>
        public bool Init(Habbo Player)
        {
            if (_permissions.Count > 0)
                _permissions.Clear();

            if (_commands.Count > 0)
                _commands.Clear();

            _permissions.AddRange(OblivionServer.GetGame().GetPermissionManager().GetPermissionsForPlayer(Player));
            _commands.AddRange(OblivionServer.GetGame().GetPermissionManager().GetCommandsForPlayer(Player));
            return true;
        }

        /// <summary>
        ///     Checks if the user has the specified right.
        /// </summary>
        /// <param name="Right"></param>
        /// <returns></returns>
        public bool HasRight(string Right) => _permissions.Contains(Right);

        /// <summary>
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public bool HasCommand(string Command) => _commands.Contains(Command);

        /// <summary>
        ///     Dispose of the permissions list.
        /// </summary>
        public void Dispose() => _permissions.Clear();
    }
}