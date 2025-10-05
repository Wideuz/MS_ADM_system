using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;


namespace AdminCommands
{
    internal class DatabaseExample
    {
        private SqliteConnection _connection;
        public DatabaseExample(string databasePath)
        {
            _connection = new SqliteConnection($"Data Source={databasePath}");
            _connection.Open();
            InitializeDatabase();
        }


    }
}
