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
    public partial class Targets : Page
    {
        public Targets()
        {
            InitializeComponent();
            LoadComboBox();
            LoadTypeComboBox();
        }
        private void LoadComboBox()
        {
            int owner = GetLoginOwner();
            List<string> options = GetComboBoxOptions(owner);
            if (options.Count == 0)
            {
                MessageBox.Show("No accounts were found.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AccountComboBox.ItemsSource = options;
            }
        }
        private void LoadTypeComboBox()
        {
            List<String> types = new List<string>()
            {
                "Income Above",
                "Expenses Below"
            };
            TargetComboBox.ItemsSource = types;
        }
        public List<string> GetComboBoxOptions(int owner)
        {
            List<string> accountOptions = new List<string>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT AccountNickname FROM accounts WHERE Owner = @Owner";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string accountName = reader.GetString(0);
                                    accountOptions.Add(accountName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return accountOptions;
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountComboBox.SelectedItem == null || TargetComboBox.SelectedItem == null ||
                AmountTextBox.Text == "" || TargetDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please complete all required fields");
                return;
            }
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Target amount must be a positive decimal value.");
                return;
            }
            int owner = GetLoginOwner();
            string account_name = AccountComboBox.SelectedItem.ToString();
            string accountfk = GetAccountFK(account_name, owner);
            string type = TargetComboBox.SelectedItem.ToString();
            decimal target_amount = Convert.ToDecimal(AmountTextBox.Text);
            DateTime target_date_input = TargetDatePicker.SelectedDate.Value;
            DateOnly target_date = DateOnly.FromDateTime(target_date_input);
            DateOnly start_date;
            if (StartDatePicker.SelectedDate != null)
            {
                DateTime start_date_input = StartDatePicker.SelectedDate.Value;
                start_date = DateOnly.FromDateTime(start_date_input);
                if (start_date > target_date)
                {
                    MessageBox.Show("Start date must be before target date");
                    return;
                }
            }
            else
            {
                start_date = DateOnly.FromDateTime(DateTime.Now);
            }
            using (var dbHelper = new DatabaseHelper())
            {
                try
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "INSERT INTO targets (ownerfk, accountfk, type, amount, startdate, targetdate) " +
                                       "VALUES (@owner, @account, @type, @amount, @start_date, @target_date)";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@owner", owner);
                            command.Parameters.AddWithValue("@account", accountfk);
                            command.Parameters.AddWithValue("@type", type);
                            command.Parameters.AddWithValue("@amount", amount);
                            command.Parameters.AddWithValue("@start_date", start_date);
                            command.Parameters.AddWithValue("@target_date", target_date);
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Target Set");
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show("Error setting target: " + ex.Message);
                }
            }
        }
        private string GetAccountFK(string account_name, int owner)
        {
            int account_int = 0;
            if (account_name == "Apply to all accounts")
            {
                return "*";
            }
            else
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    try
                    {
                        using (var connection = dbHelper.GetConnection())
                        {
                            string query = "SELECT AccountPK FROM accounts WHERE AccountNickname = @account_name AND Owner = @owner";
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@account_name", account_name);
                                command.Parameters.AddWithValue("@owner", owner);
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        account_int = reader.GetInt32(0);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error finding account on database: " + ex.Message);
                    }
                }
                string accountfk = account_int.ToString();
                return accountfk;
            }
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }

        private void TargetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
