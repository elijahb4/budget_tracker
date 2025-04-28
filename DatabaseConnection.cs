using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;
using System.Windows;

namespace Individual_project_initial
{
    public class DatabaseHelper : IDisposable
    {
        private readonly string _connectionString;
        private NpgsqlConnection _connection;
        private bool _disposed = false;

        public DatabaseHelper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("DB_Connection.json", optional: false, reloadOnChange: true)
                .Build();

            var host = config["Host"];
            var port = config["Port"];
            var database = config["Database"];
            var username = config["Username"];
            var password = config["Password"];

            host = host?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("One or more database configuration values are missing in DB_Connection.json.");
            }

            _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        public NpgsqlConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    _connection.Open();
                    MessageBox.Show("Database connection established.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database connection failed: " + ex.Message);
                    throw;
                }
            }

            return _connection;
        }

        public void SetupSchema()
        {
            var sqlPath = Path.Combine(AppContext.BaseDirectory, "Sql", "init_schema.sql");

            if (!File.Exists(sqlPath))
            {
                MessageBox.Show("Schema SQL file not found at: " + sqlPath);
                return;
            }

            var sql = File.ReadAllText(sqlPath);

            try
            {
                using var cmd = new NpgsqlCommand(sql, GetConnection());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Database schema successfully ensured.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute schema setup: " + ex.Message);
            }

            SeedData();
        }

        private void SeedData()
        {
            var seedDataPath = Path.Combine(AppContext.BaseDirectory, "Sql", "seed_data.sql");

            if (!File.Exists(seedDataPath))
            {
                MessageBox.Show("Seed data SQL file not found at: " + seedDataPath);
                return;
            }

            var seedDataSql = File.ReadAllText(seedDataPath);

            try
            {
                using var cmd = new NpgsqlCommand(seedDataSql, GetConnection());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Test data successfully inserted.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert test data: " + ex.Message);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }

                _disposed = true;
            }
        }
    }
}
