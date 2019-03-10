#region

using MySql.Data.MySqlClient;
using System.Data;

#endregion

namespace Oblivion.Database.Interfaces
{
    public interface IRegularQueryAdapter
    {
        void AddParameter(string name, object query);
        bool FindsResult();
        int GetInteger();
        DataRow GetRow();
        string GetString();
        DataTable GetTable();
        void RunFastQuery(string query);
        string GetCommand();
        MySqlDataReader ExecuteReader();
        void RunQuery(string query);
        void SetQuery(string query);
    }
}