using IndividualProjectInitial;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Individual_project_initial
{
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            GetLoginOwner();
        }

        private void ChangeUsername_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = UsernameTextBox.Text;
            if (string.IsNullOrEmpty(newUsername))
            {
                MessageBox.Show("Please enter a new username before trying to update");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE user_information SET username = @newUsername WHERE user_pk = @userId";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newUsername", newUsername);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            //add hashing later
            string password_input = txtPassword.Password;
            if (string.IsNullOrEmpty(password_input))
            {
                MessageBox.Show("Please enter a valid password before trying to update.");
                return;
            }
            if (password_input.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE user_information SET password = @newPassword WHERE user_pk = @userId";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newPassword", password_input);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating password: " + ex.Message);
            }
        }

        private void ChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            string email_input = EmailTextBox.Text;
            if (string.IsNullOrEmpty(email_input))
            {
                MessageBox.Show("Please enter a new email before trying to update");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE user_information SET email = @newEmail WHERE user_pk = @userId";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newEmail", email_input);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating email: " + ex.Message);
            }
        }

        private void ChangeTaxFree_Click(object sender, RoutedEventArgs e)
        {
            string tax_allowance_input = TaxFreeTextBox.Text;
            if (string.IsNullOrEmpty(tax_allowance_input))
            {
                MessageBox.Show("Please enter a valid tax allowance.");
                return;
            }
            if (!IsValidDecimal(tax_allowance_input))
            {
                MessageBox.Show("Please enter a valid decimal number for the tax allowance.");
                return;
            }
            decimal.Parse(tax_allowance_input);
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE user_information SET tax_allowance = @newTaxAllowance WHERE user_pk = @userId";

                        using (var command = new NpgsqlCommand(query, connection))
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating tax allowance: " + ex.Message);
            }
        }
        static bool IsValidDecimal(string input)
        {
            return decimal.TryParse(input, out _);
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
