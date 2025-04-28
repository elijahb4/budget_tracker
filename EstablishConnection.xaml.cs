using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Individual_project_initial.Properties;
using Npgsql;

namespace Individual_project_initial
{
    public partial class EstablishConnection : Window
    {
        public EstablishConnection()
        {
            InitializeComponent();
            SetDefaultPwd();
        }

        private void SetDefaultPwd()
        {
            string defalutPwd = "administrator";
            passwordBox.Password = defalutPwd;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = BuildConnectionString();
            bool test = TestConnection(connectionString);
            if (test == true)
            {
                MessageBox.Show("Connection Suceeded!");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = BuildConnectionString();
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            string host = builder.Host;
            string port = builder.Port.ToString();
            string database = builder.Database;
            string username = builder.Username;
            string password = builder.Password;

            var config = new DB_Connection
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password
            };

            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("DB_Connection.json", jsonString);
            using (var dbHelper = new DatabaseHelper())
            {
                dbHelper.SetupSchema();
            }
            this.Hide();
            Login loginWindow = new Login();
            loginWindow.Show();
        }

        string BuildConnectionString()
        {
            string host = hostTextBox.Text;
            string port = portTextBox.Text;
            string database = databaseTextBox.Text;
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};Pooling=true";
        }
        bool TestConnection(string connectionString)
        {
            try
            {
                using var conn = new Npgsql.NpgsqlConnection(connectionString);
                conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}");
                return false;
            }
        }
        public void PerformDatabaseOperation()
        {
            using (var dbHelper = new DatabaseHelper())
            {
                using (var connection = dbHelper.GetConnection())
                {

                }
            }
        }
    }
}
