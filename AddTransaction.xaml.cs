using System;
using Npgsql;
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

namespace Individual_project_initial
{
    public partial class AddTransaction : Page
    {
        public AddTransaction()
        {
            InitializeComponent();
            DataContext = this;
            LoadComboBox();
            PopulateHourComboBox();
            PopulateMinuteComboBox();
        }
        private void LoadComboBox()
        {
            int owner = GetLoginOwner();
            List<string> options = GetComboBoxOptions(owner);
            if (options.Count == 0)
            {
                MessageBox.Show("No options found for the ComboBox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AccountComboBox.ItemsSource = options;
                Console.WriteLine("ComboBox options loaded successfully.");
            }
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
                        string query = "SELECT accountnickname FROM accounts WHERE owner = @Owner";

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
        private void PopulateHourComboBox()
        {
            for (int i = 0; i < 24; i++)
            {
                HourComboBox.Items.Add(i.ToString("D2"));
            }
            HourComboBox.SelectedIndex = 0;
        }
        private void PopulateMinuteComboBox()
        {
            for (int i = 0; i < 60; i++)
            {
                MinuteComboBox.Items.Add(i.ToString("D2"));
            }
            MinuteComboBox.SelectedIndex = 0;
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedAccount = AccountComboBox.SelectedItem.ToString();
            int accountPK = GetAccountPK(selectedAccount);
            decimal Balance = GetAccountBalance(accountPK);
            DateTime transactionDate = DateComboBox.SelectedDate.Value;
            int selectedHour = int.Parse(HourComboBox.SelectedItem.ToString());
            int selectedMinute = int.Parse(MinuteComboBox.SelectedItem.ToString());
            string transactionSumInput = TransactionSumBox.Text;
            decimal transactionSum;
            if (IsValidDecimal(transactionSumInput))
            {
                transactionSum = decimal.Parse(transactionSumInput);
            }
            else
            {
                MessageBox.Show("Please enter a valid decimal number for the transaction sum.");
                return;
            }
            string note = NoteBox.Text;
            DateTime transactionTime = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, selectedHour, selectedMinute, 0);
            decimal BalanceAfter = Balance + transactionSum;
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO transactions (transactionSum, transactionTime, accountfk, balanceafter, balanceprior)
                        VALUES (@sum, @time, @accountFK, @balanceafter, @balanceprior)";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@sum", transactionSum);
                            command.Parameters.AddWithValue("@time", transactionTime);
                            command.Parameters.AddWithValue("@accountFK", accountPK);
                            command.Parameters.AddWithValue("@balanceafter", BalanceAfter);
                            command.Parameters.AddWithValue("@balanceprior", Balance);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Transaction added successfully!");
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Error adding transaction: " + ex.Message);
            }
        }
        private int GetAccountPK(string selectedAccount)
        {
            int accountPK = 0;
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT accountpk FROM accounts WHERE accountnickname = @accountnickname";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@accountnickname", selectedAccount);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    accountPK = reader.GetInt32(0);
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
            return accountPK;
        }

        private decimal GetAccountBalance(int accountPK)
        {
            decimal Balance = 0;
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT balance FROM accounts WHERE accountpk = @accountpk";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@accountpk", accountPK);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    Balance = reader.GetDecimal(0);
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
            return Balance;
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