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
    public partial class UpdateAccountDetails : Page
    {
        public List<string> AccountTypes { get; private set; }

        public UpdateAccountDetails()
        {
            InitializeComponent();
            AccountTypes = new List<string>();
            PopulateComboBoxWithAccountTypes();
        }

        private void PopulateComboBoxWithAccountTypes()
        {
            List<string> accountTypes = GetComboBoxOptions();
            if (accountTypes.Count == 0)
            {
                MessageBox.Show("No options found for the ComboBox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AccountTypeComboBox.ItemsSource = accountTypes;
                Console.WriteLine($"ComboBox loaded with {accountTypes.Count} options.");
            }
        }


        public List<string> GetComboBoxOptions()
        {
            return new List<string>
            {
                "Current",
                "Savings",
                "ISA"
            };
        }

        public string SelectedAccountType { get; set; }

        private void UpdateInsName_Click(object sender, RoutedEventArgs e)
        {
            string newValue = institutionNameTextBox.Text;
            if (string.IsNullOrEmpty(newValue))
            {
                MessageBox.Show("Please enter a value");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE accounts SET institutionname = @newInsName WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newInsName", newValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void UpdateNickname_Click(object sender, RoutedEventArgs e)
        {
            string newValue = accountNameTextBox.Text;
            if (string.IsNullOrEmpty(newValue))
            {
                MessageBox.Show("Please enter a value");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE accounts SET accountnickname = @newNickname WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newNickname", newValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void UpdateAccNum_Click(object sender, RoutedEventArgs e)
        {
            string newValue = accountNumberTextBox.Text;
            if (string.IsNullOrEmpty(newValue))
            {
                MessageBox.Show("Please enter a value");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE accounts SET accountnumber = @newAccNum WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newAccNum", newValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void UpdateSortCode_Click(object sender, RoutedEventArgs e)
        {
            string newValue = sortCodeTextBox.Text;
            if (string.IsNullOrEmpty(newValue))
            {
                MessageBox.Show("Please enter a value");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE accounts SET institutionname = @newInsName WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newInsName", newValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void UpdateReference_Click(object sender, RoutedEventArgs e)
        {
            string newValue = referenceTextBox.Text;
            if (string.IsNullOrEmpty(newValue))
            {
                MessageBox.Show("Please enter a value");
                return;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE accounts SET institutionname = @newInsName WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newInsName", newValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void UpdateType_Click(object sender, RoutedEventArgs e)
        {
            if (AccountTypeComboBox.SelectedItem != null)
            {
                SelectedAccountType = AccountTypeComboBox.SelectedItem.ToString() ?? string.Empty;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE accounts SET accounttype = @newType WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newType", SelectedAccountType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}