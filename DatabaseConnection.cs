using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;

namespace Individual_project_initial
{
    public class DatabaseHelper : IDisposable
    {
        private readonly string _connectionString;
        private MySqlConnection _connection;
        private bool _disposed = false;

        public DatabaseHelper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = config.GetConnectionString("LocalDatabase");
        }

        public MySqlConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new MySqlConnection(_connectionString);
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