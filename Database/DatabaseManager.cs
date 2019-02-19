#region

using System;
using MySql.Data.MySqlClient;
using Oblivion.Database.Interfaces;

#endregion

namespace Oblivion.Database
{
    public sealed class DatabaseManager
    {
        private readonly string _connectionStr;

        public DatabaseManager(string ConnectionStr)
        {
            _connectionStr = ConnectionStr;
        }

        public bool IsConnected()
        {
            try
            {
                var Con = new MySqlConnection(_connectionStr);
                Con.Open();
                var CMD = Con.CreateCommand();
                CMD.CommandText = "SELECT 1+1";
                CMD.ExecuteNonQuery();

                CMD.Dispose();
                Con.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public IQueryAdapter GetQueryReactor()
        {
            try
            {
                IDatabaseClient DbConnection = new DatabaseConnection(_connectionStr);

                DbConnection.connect();

                return DbConnection.GetQueryReactor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}