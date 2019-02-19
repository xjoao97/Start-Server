#region

using System;
using MySql.Data.MySqlClient;

#endregion

namespace Oblivion.Database.Interfaces
{
    public interface IDatabaseClient : IDisposable
    {
        void connect();
        void disconnect();
        IQueryAdapter GetQueryReactor();
        MySqlCommand createNewCommand();
        void reportDone();
    }
}