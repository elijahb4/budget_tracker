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
            //GetLoginOwner();
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
            string newInsName = institutionNameTextBox.Text;
            if (string.IsNullOrEmpty(newInsName))
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
                            command.Parameters.AddWithValue("@newInsName", newInsName);
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
            string newInsName = accountNameTextBox.Text;
            if (string.IsNullOrEmpty(newInsName))
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
                            command.Parameters.AddWithValue("@newInsName", newInsName);
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
            string newInsName = accountNumberTextBox.Text;
            if (string.IsNullOrEmpty(newInsName))
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
                            command.Parameters.AddWithValue("@newInsName", newInsName);
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
            string newInsName = sortCodeTextBox.Text;
            if (string.IsNullOrEmpty(newInsName))
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
                            command.Parameters.AddWithValue("@newInsName", newInsName);
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
            string newInsName = referenceTextBox.Text;
            if (string.IsNullOrEmpty(newInsName))
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
                            command.Parameters.AddWithValue("@newInsName", newInsName);
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
                        string query = "UPDATE accounts SET institutionname = @newInsName WHERE accountpk = @accountpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@newInsName", SelectedAccountType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}