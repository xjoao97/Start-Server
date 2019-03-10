#region

using System;
using System.Threading;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Global
{
    public class ServerStatusUpdater : IDisposable
    {
        private const int UpdateInSecs = 30;
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.Core.ServerUpdater");

        private static int _userPeak, _lastDatePeak;

        private static string _lastDate;

        private Timer _timer;

        public ServerStatusUpdater()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT userpeak FROM server_status");
                _userPeak = dbClient.getInteger();

                _lastDate = DateTime.Now.ToShortDateString();
                dbClient.SetQuery("SELECT count FROM server_peaks WHERE date = '" + _lastDate + "'");
                _lastDatePeak = dbClient.getInteger();
            }
        }


        public void Dispose()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE server_status SET users_online = '0', loaded_rooms = '0', status = '0'");
            }

            _timer.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Init()
        {
            _timer = new Timer(OnTick, null, TimeSpan.FromSeconds(UpdateInSecs), TimeSpan.FromSeconds(UpdateInSecs));

            Console.Title = "Oblivion Emulator - 0 usuários online - 0 quartos carregados - 0 dia(s) 0 hora(s) ligado";

            Log.Info("Server Status Updater has been started.");
        }

        public void OnTick(object Obj) => UpdateOnlineUsers();

        private static void UpdateOnlineUsers()
        {
            var Uptime = DateTime.Now - OblivionServer.ServerStarted;

            var UsersOnline = Convert.ToInt32(OblivionServer.GetGame().GetClientManager().Count);
            var RoomCount = OblivionServer.GetGame().GetRoomManager().Count;
            Game.SessionUserRecord = UsersOnline > Game.SessionUserRecord ? UsersOnline : Game.SessionUserRecord;

            Console.Title = "Oblivion Emulator - " + UsersOnline + " usuários online - " + RoomCount +
                            " quartos carregados - " +
                            Uptime.Days + " dia(s) " + Uptime.Hours + " hora(s) uptime";

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                if (UsersOnline > _userPeak)
                    _userPeak = UsersOnline;

                if (_lastDate == DateTime.Now.ToShortDateString() && UsersOnline > _lastDatePeak ||
                    _lastDate != DateTime.Now.ToShortDateString())
                    dbClient.RunFastQuery("REPLACE INTO server_peaks VALUES ('" + DateTime.Now.ToShortDateString() + "', " +
                                      UsersOnline + ")");

                _lastDate = DateTime.Now.ToShortDateString();
                dbClient.SetQuery(
                    "UPDATE server_status SET users_online = @users, loaded_rooms = @loadedRooms, userpeak = @upeak LIMIT 1;");
                dbClient.AddParameter("users", UsersOnline);
                dbClient.AddParameter("loadedRooms", RoomCount);
                dbClient.AddParameter("upeak", _userPeak);
                dbClient.RunQuery();
            }
        }
    }
}