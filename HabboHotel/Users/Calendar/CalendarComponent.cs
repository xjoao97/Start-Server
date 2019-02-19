#region

using System;
using System.Collections.Generic;
using System.Data;

#endregion

namespace Oblivion.HabboHotel.Users.Calendar
{
    /// <summary>
    ///     Permissions for a specific Player.
    /// </summary>
    public sealed class CalendarComponent
    {
        /// <summary>
        ///     Permission rights are stored here.
        /// </summary>
        private readonly List<int> _lateBoxes;

        private readonly List<int> _openedBoxes;

        public CalendarComponent()
        {
            _lateBoxes = new List<int>();
            _openedBoxes = new List<int>();
        }

        /// <summary>
        ///     Init the PermissionComponent.
        /// </summary>
        /// <param name="Player"></param>
        public bool Init(Habbo Player)
        {
            _lateBoxes.Clear();

            _openedBoxes.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT status,day FROM `user_xmas15_calendar` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", Player.Id);
                var GetData = dbClient.getTable();

                if (GetData != null)
                    foreach (DataRow Row in GetData.Rows)
                        if (Convert.ToInt32(Row["status"]) == 0)
                            _lateBoxes.Add(Convert.ToInt32(Row["day"]));
                        else
                            _openedBoxes.Add(Convert.ToInt32(Row["day"]));
            }
            return true;
        }

        public List<int> GetOpenedBoxes() => _openedBoxes;

        public List<int> GetLateBoxes() => _lateBoxes;

        /// <summary>
        ///     Dispose of the permissions list.
        /// </summary>
        public void Dispose()
        {
            _lateBoxes.Clear();
            _openedBoxes.Clear();
        }
    }
}