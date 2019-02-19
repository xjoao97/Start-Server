#region

using System;
using System.Data;
using MySql.Data.MySqlClient;
using Oblivion.Database.Adapter;
using Oblivion.Database.Interfaces;

#endregion

namespace Oblivion.Database
{
    public class DatabaseConnection : IDatabaseClient, IDisposable
    {
        private readonly IQueryAdapter _adapter;
        private readonly MySqlConnection _con;

        public DatabaseConnection(string ConnectionStr)
        {
            _con = new MySqlConnection(ConnectionStr);
            _adapter = new NormalQueryReactor(this);
        }

        public void Dispose()
        {
            if (_con.State == ConnectionState.Open)
                _con.Close();

            // _con.Dispose();
            GC.SuppressFinalize(this);
        }

        public void connect() => Open();

        public void disconnect() => Close();

        public IQueryAdapter GetQueryReactor() => _adapter;

        public void reportDone() => Dispose();

        public MySqlCommand createNewCommand() => _con.CreateCommand();

        public void Open()
        {
            try
            {
                if (_con.State == ConnectionState.Closed)
                    _con.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Close()
        {
            try
            {
                if (_con.State == ConnectionState.Open)
                    _con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}