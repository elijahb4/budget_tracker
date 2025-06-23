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
using Npgsql;
using static System.TimeZoneInfo;

namespace Individual_project_initial
{
    public partial class Transfer : Page
    {
        public Transfer()
        {
            InitializeComponent();
            LoadComboBox();
        }

        //Load two comboboxes, one for the account to and one for the account from

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
                AccountFromComboBox.ItemsSource = options;
                AccountToComboBox.ItemsSource = options;
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

        // Submit event handler, if the same a account is selected twice the user is returned and no operation is performed
        private void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedFromAccount = AccountFromComboBox.SelectedItem.ToString();
            string selectedToAccount = AccountToComboBox.SelectedItem.ToString();
            if (selectedFromAccount == selectedToAccount)
            {
                MessageBox.Show("Please select different accounts for transfer.");
                return;
            }
            int accountFromPK = GetAccountPK(selectedFromAccount);
            int accountToPK = GetAccountPK(selectedToAccount);
            decimal FromBalance = GetAccountBalance(accountFromPK);
            decimal ToBalance = GetAccountBalance(accountToPK);
            string transactionSumInput = SumTextBox.Text;
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
            // two transactions to be inserted (account to and account from)
            bool deduct = true;
            InsertTransaction(accountFromPK, transactionSum, FromBalance, DateTime.Now, deduct);
            deduct = false;
            InsertTransaction(accountToPK, transactionSum, FromBalance, DateTime.Now, deduct);
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
                        string query = "SELECT accountpk FROM accounts WHERE accountnickname = @AccountNickname";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@AccountNickname", selectedAccount);
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

        private void InsertTransaction (int accountpk, decimal sum, decimal balance, DateTime time, bool deduct)
        {
            decimal balanceAfter;
            if (deduct)
            {
                balanceAfter = balance - sum;
            }
            else
            {
                balanceAfter = balance + sum;
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO transactions (transactionsum, transactiontime, accountfk, balanceafter, balanceprior, logtype)
                        VALUES (@sum, @time, @accountFK, @balanceafter, @balanceprior, @logtype)";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@sum", sum);
                            command.Parameters.AddWithValue("@time", time);
                            command.Parameters.AddWithValue("@accountFK", accountpk);
                            command.Parameters.AddWithValue("@balanceafter", balanceAfter);
                            command.Parameters.AddWithValue("@balanceprior", balance);
                            command.Parameters.AddWithValue("@logtype", "Transfer");
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