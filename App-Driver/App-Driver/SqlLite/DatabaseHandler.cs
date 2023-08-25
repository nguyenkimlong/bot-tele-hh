using HelloBotNET.AppService.SqlLite.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloBotNET.AppService.Database
{
    public class DatabaseHandler
    {
        private SQLiteConnection _db;

        public DatabaseHandler()
        {
            var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "MyData.db");

            _db = new SQLiteConnection(databasePath);
            _db.CreateTable<Employee>();
        }
    }
}
