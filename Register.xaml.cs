using Mysqlx.Crud;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Individual_project_initial
{
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (nameTextBox == null || usernameTextBox == null || emailTextBox == null || PasswordBox == null || PasswordConfirmBox == null)
            {
                MessageBox.Show("All fields must have a value");
                return;
            }

                if (PasswordBox.Password != PasswordConfirmBox.Password)
                {
                    MessageBox.Show("Passwords do not match");
                    return;
                }

                if (PasswordBox.Password.Length < 8)
                {
                    MessageBox.Show("Password must be at least 8 characters long");
                    return;
                }
                if (usernameTextBox.Text.Length < 4)
                {
                    MessageBox.Show("Username must be at least 4 characters long");
                    return;
                }

            string username = usernameTextBox.Text;
            string email = emailTextBox.Text;
            string password = PasswordBox.Password;
            string hash = PasswordHasher.Hash(password);

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "INSERT INTO user_information (username, email, password) VALUES (@username, @email, @passwordhash)";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@email", email);
                            command.Parameters.AddWithValue("@passwordhash", hash);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating account on database: " + ex.Message);
            }

            this.Hide();
            Login loginWindow = new Login();
            loginWindow.Show();
        }
    }
}
