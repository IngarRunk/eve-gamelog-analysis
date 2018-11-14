using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace eve_gamelog_analysis {
    public class Database {
        public string ConnectionString { get; set; }

        private MySql.Data.MySqlClient.MySqlConnection _connection = null;

        public void UpdateEvents(IEnumerable<CynoEvent> cynoEvents) { 
            if (_connection == null) {
                _connection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
            }
            if (_connection.State != System.Data.ConnectionState.Open) {
                _connection.Open();
            }

            var delete = _connection.CreateCommand();
            delete.CommandText = "DELETE FROM CynoEvents";
            delete.ExecuteNonQuery();

            foreach (var evt in cynoEvents)
            {
                var cmd = _connection.CreateCommand();
                cmd.CommandText = "INSERT INTO CynoEvents (`Date`, `Character`, `System`, `Station`) VALUES (@d, @char, @sys, @station)";
                cmd.Parameters.AddWithValue("@d", evt.When);
                cmd.Parameters.AddWithValue("@char", evt.Character);
                cmd.Parameters.AddWithValue("@sys", evt.System);
                cmd.Parameters.AddWithValue("@station", evt.Station);
                cmd.ExecuteNonQuery();
            }
            _connection.Close();
        }
    }
}