#region

using System;
using System.Linq;
using System.Threading;
using Oblivion.Core;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Cache.Process
{
    internal sealed class ProcessComponent
    {
        //  private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Cache.Process.ProcessComponent");

        /// <summary>
        ///     How often the timer should execute.
        /// </summary>
        private const int RuntimeInSec = 1200;

        /// <summary>
        ///     Used for disposing the ProcessComponent safely.
        /// </summary>
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        /// <summary>
        ///     Enable/Disable the timer WITHOUT disabling the timer itself.
        /// </summary>
        private bool _disabled;

        /// <summary>
        ///     ThreadPooled Timer.
        /// </summary>
        private Timer _timer;

        /// <summary>
        ///     Checks if the timer is lagging behind (server can't keep up).
        /// </summary>
        //private bool _timerLagging;
        /// <summary>
        ///     Prevents the timer from overlapping itself.
        /// </summary>
        private bool _timerRunning;

        /// <summary>
        ///     Initializes the ProcessComponent.
        /// </summary>
        public void Init() => _timer = new Timer(Run, null, RuntimeInSec * 1000, RuntimeInSec * 1000);

        /// <summary>
        ///     Called for each time the timer ticks.
        /// </summary>
        /// <param name="State"></param>
        public void Run(object State)
        {
            try
            {
                if (_disabled)
                    return;

                if (_timerRunning)
                    return;

                _resetEvent.Reset();

                // BEGIN CODE
                var CacheList = OblivionServer.GetGame().GetCacheManager().GetUserCache();
                if (CacheList.Count > 0)
                    foreach (var Cache in CacheList.OfType<UserCache>().ToList())
                        try
                        {
                            if (Cache == null)
                                continue;


                            if (Cache.isExpired())
                                OblivionServer.GetGame().GetCacheManager().TryRemoveUser(Cache.Id);
                        }
                        catch (Exception e)
                        {
                            Logging.LogCacheException(e.ToString());
                        }

                CacheList.Clear();

                var CachedUsers = OblivionServer.GetUsersCached().ToList();
                if (CachedUsers.Count > 0)
                    foreach (var Data in CachedUsers)
                        try
                        {
                            if (Data == null)
                                continue;

                            Habbo Temp = null;

                            if (Data.CacheExpired())
                                OblivionServer.RemoveFromCache(Data.Id, out Temp);

                            Temp?.Dispose();

                            //   Temp = null;
                        }
                        catch (Exception e)
                        {
                            Logging.LogCacheException(e.ToString());
                        }
                CachedUsers.Clear();

                // END CODE

                // Reset the values
                _timerRunning = false;
                //  _timerLagging = false;

                _resetEvent.Set();
            }
            catch (Exception e)
            {
                Logging.LogCacheException(e.ToString());
            }
        }

        /// <summary>
        ///     Stops the timer and disposes everything.
        /// </summary>
        public void Dispose()
        {
            // Wait until any processing is complete first.
            try
            {
                _resetEvent.WaitOne(TimeSpan.FromMinutes(5));
            }
            catch
            {
            } // give up

            // Set the timer to disabled
            _disabled = true;

            // Dispose the timer to disable it.
            try
            {
                _timer?.Dispose();
            }
            catch
            {
            }

            // Remove reference to the timer.
            _timer = null;
        }
    }
}