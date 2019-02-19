#region

using System;
using System.Threading;
using log4net;
using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Handshake;

#endregion

namespace Oblivion.HabboHotel.Users.Process
{
    internal sealed class ProcessComponent
    {
        /// <summary>
        ///     How often the timer should execute.
        /// </summary>
        private const int RuntimeInSec = 60;

        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Users.Process.ProcessComponent");

        /// <summary>
        ///     Used for disposing the ProcessComponent safely.
        /// </summary>
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        /// <summary>
        ///     Enable/Disable the timer WITHOUT disabling the timer itself.
        /// </summary>
        private bool _disabled;

        /// <summary>
        ///     Player to update, handle, change etc.
        /// </summary>
        private Habbo _player;

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
        /// <param name="Player">Player.</param>
        public bool Init(Habbo Player)
        {
            if (Player == null)
                return false;
            if (_player != null)
                return false;

            _player = Player;
            _timer = new Timer(Run, null, RuntimeInSec * 1000, RuntimeInSec * 1000);
            return true;
        }

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
                {
                    log.Warn("<Player " + _player.Id + "> Server can't keep up, Player timer is lagging behind.");
                    return;
                }

                _resetEvent.Reset();

                // BEGIN CODE

                #region Muted Checks

                if (_player.TimeMuted > 0)
                    _player.TimeMuted -= 60;

                #endregion

                #region Console Checks

                if (_player.MessengerSpamTime > 0)
                    _player.MessengerSpamTime -= 60;
                if (_player.MessengerSpamTime <= 0)
                    _player.MessengerSpamCount = 0;

                #endregion

                _player.TimeAFK += 1;

                #region Respect checking

                if (_player.GetStats().RespectsTimestamp != DateTime.Today.ToString("MM/dd"))
                {
                    _player.GetStats().RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery(
                            "UPDATE `user_stats` SET `dailyRespectPoints` = '3', `dailyPetRespectPoints` = '3', `respectsTimestamp` = '" +
                            DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + _player.Id +
                            "' LIMIT 1");
                    }

                    _player.GetStats().DailyRespectPoints = 3;
                    _player.GetStats().DailyPetRespectPoints = 3;

                    if (_player.GetClient() != null)
                        _player.GetClient().SendMessage(new UserObjectComposer(_player));
                }

                #endregion

                #region Reset Scripting Warnings

                if (_player.GiftPurchasingWarnings < 15)
                    _player.GiftPurchasingWarnings = 0;

                if (_player.MottoUpdateWarnings < 15)
                    _player.MottoUpdateWarnings = 0;

                if (_player.ClothingUpdateWarnings < 15)
                    _player.ClothingUpdateWarnings = 0;

                #endregion

                if (_player.GetClient() != null)
                    OblivionServer.GetGame()
                        .GetAchievementManager()
                        .ProgressAchievement(_player.GetClient(), "ACH_AllTimeHotelPresence", 1);

                _player.CheckCreditsTimer();
                _player.Effects().CheckEffectExpiry(_player);

                // END CODE
                _player.GetClient()
                    .SendMessage(
                        new UnreadForumThreadPostsComposer(
                            OblivionServer.GetGame().GetGroupForumManager().GetUnreadThreadForumsByUserId(_player.Id)));
                // Reset the values
                _timerRunning = false;

                _resetEvent.Set();
            }
            catch
            {
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

            // Null the player so we don't reference it here anymore
            _player = null;
        }
    }
}