using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace winforms_templates
{
    public class SqliteHelper<T> where T : new()
    {
        private string _connectionString;

        public SqliteHelper()
        {
            _connectionString = "Data Source=db/config.db";
        }

        public void Insert(T model)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var properties = typeof(T).GetProperties()
                    .Where(p => p.Name != "ID" && p.GetValue(model) != null);

                var columns = string.Join(", ", properties.Select(p => p.Name));
                var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

                var insertQuery = $"INSERT INTO {typeof(T).Name} ({columns}) VALUES ({values})";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    foreach (var property in properties)
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(model));
                    }
                    command.ExecuteNonQuery();
                }
            }
        }


        public List<T> GetAll()
        {
            var results = new List<T>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var selectQuery = $"SELECT * FROM {typeof(T).Name}";

                using (var command = new SQLiteCommand(selectQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var model = new T();
                        foreach (var property in typeof(T).GetProperties())
                        {
                            var columnName = property.Name;
                            if (reader[columnName] != DBNull.Value)
                            {
                                var value = reader[columnName];
                                property.SetValue(model, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                        results.Add(model);
                    }
                }
            }

            return results;
        }
        public List<T> GetByColumnValue(string columnName, object value)
        {
            var results = new List<T>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var selectQuery = $"SELECT * FROM {typeof(T).Name} WHERE {columnName} = @Value";

                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Value", value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var model = new T();
                            foreach (var property in typeof(T).GetProperties())
                            {
                                if (reader[property.Name] != DBNull.Value)
                                {
                                    var columnValue = reader[property.Name];
                                    property.SetValue(model, Convert.ChangeType(columnValue, property.PropertyType));
                                }
                            }
                            results.Add(model);
                        }
                    }
                }
            }

            return results;
        }


        public void Update(T model)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var properties = typeof(T).GetProperties();
                var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

                var updateQuery = $"UPDATE {typeof(T).Name} SET {setClause} WHERE Id = @Id";

                using (var command = new SQLiteCommand(updateQuery, connection))
                {
                    foreach (var property in properties)
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(model));
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var deleteQuery = $"DELETE FROM {typeof(T).Name} WHERE Id = @Id";

                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string GetSqliteType(Type type)
        {
            if (type == typeof(int)) return "INTEGER";
            if (type == typeof(string)) return "TEXT";
            if (type == typeof(double)) return "REAL";
            if (type == typeof(bool)) return "INTEGER";
            // Add more type mappings as needed
            throw new NotSupportedException($"Type {type} not supported");
        }
    }
}
