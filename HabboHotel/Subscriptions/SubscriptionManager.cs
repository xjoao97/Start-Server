#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;

#endregion

namespace Oblivion.HabboHotel.Subscriptions
{
    public class SubscriptionManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.Subscriptions.SubscriptionManager");

        private readonly Dictionary<int, SubscriptionData> _subscriptions = new Dictionary<int, SubscriptionData>();

        public void Init()
        {
            if (_subscriptions.Count > 0)
                _subscriptions.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `subscriptions`;");
                var GetSubscriptions = dbClient.getTable();

                if (GetSubscriptions != null)
                    foreach (
                        var Row in
                        GetSubscriptions.Rows.Cast<DataRow>()
                            .Where(Row => !_subscriptions.ContainsKey(Convert.ToInt32(Row["id"]))))
                        _subscriptions.Add(Convert.ToInt32(Row["id"]),
                            new SubscriptionData(Convert.ToInt32(Row["id"]), Convert.ToString(Row["name"]),
                                Convert.ToString(Row["badge_code"]), Convert.ToInt32(Row["credits"]),
                                Convert.ToInt32(Row["duckets"]), Convert.ToInt32(Row["respects"])));
            }

            log.Info("Loaded " + _subscriptions.Count + " subscriptions.");
        }

        public bool TryGetSubscriptionData(int Id, out SubscriptionData Data)
            => _subscriptions.TryGetValue(Id, out Data);
    }
}