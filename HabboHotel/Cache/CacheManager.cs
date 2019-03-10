#region

using System.Collections.Specialized;
using log4net;
using Oblivion.HabboHotel.Cache.Process;

#endregion

namespace Oblivion.HabboHotel.Cache
{
    public class CacheManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.HabboHotel.Cache.CacheManager");
        private readonly HybridDictionary _usersCached;

        public CacheManager()
        {
            _usersCached = new HybridDictionary();
            var process = new ProcessComponent();
            process.Init();
            Log.Info("Cache -> CARREGADO");
        }

        public bool ContainsUser(int Id) => _usersCached.Contains(Id);

        public UserCache GenerateUser(int Id)
        {
            UserCache User = null;
            if (_usersCached.Contains(Id))
            {
                User = TryGetUser(Id);
                return User;
            }

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Id);
            if (Client?.GetHabbo() != null)
            {
                User = new UserCache(Id, Client.GetHabbo().Username, Client.GetHabbo().Motto, Client.GetHabbo().Look);
                _usersCached.Add(Id, User);
                return User;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username`, `motto`, `look` FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", Id);

                var dRow = dbClient.GetRow();

                if (dRow != null)
                {
                    User = new UserCache(Id, dRow["username"].ToString(), dRow["motto"].ToString(),
                        dRow["look"].ToString());
                    _usersCached.Add(Id, User);
                }
            }

            return User;
        }

        public void TryRemoveUser(int Id)
        {
            if (_usersCached.Contains(Id))
                _usersCached.Remove(Id);
        }

        public UserCache TryGetUser(int Id) => (UserCache) _usersCached[Id];

        public HybridDictionary GetUserCache() => _usersCached;
    }
}