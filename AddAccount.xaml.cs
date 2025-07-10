using IndividualProjectInitial;
using MySqlX.XDevAPI.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static System.TimeZoneInfo;

namespace Individual_project_initial
{
    public partial class AddAccount : Page
    {
        public List<string> AccountTypes { get; private set; }

        public AddAccount()
        {
            InitializeComponent();
            DataContext = this;
            AccountTypes = new List<string>();
            PopulateComboBoxWithAccountTypes();
        }

        // Load the combobox with account types

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

        // Combobox selection changed event handler

        public string SelectedAccountType { get; set; }

        private void AccountTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountTypeComboBox.SelectedItem != null)
            {
                SelectedAccountType = AccountTypeComboBox.SelectedItem.ToString() ?? string.Empty;
            }
        }

        // Submit button click event handler and logic

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int owner = GetLoginOwner();
            string accountType = SelectedAccountType;

            if (string.IsNullOrEmpty(accountType))
            {
                MessageBox.Show("Please select an account type.");
                return;
            }

            string institutionName = institutionNameTextBox.Text;
            string accountName = accountNameTextBox.Text;
            string accountNumber = accountNumberTextBox.Text;
            string sortCode = sortCodeTextBox.Text;
            string reference = referenceTextBox.Text;
            string startingBalanceText = balanceTextBox.Text;
            string interestRateText = interestRateTextBox.Text;
            int accountPK = 0;
            decimal startingBalance = 0;
            decimal interestRate = 0;

            if (!decimal.TryParse(startingBalanceText, out startingBalance))
            {
                MessageBox.Show("Invalid starting balance. Please enter a valid number.");
                return;
            }
            if (!decimal.TryParse(interestRateText, out interestRate))
            {
                MessageBox.Show("Invalid starting balance. Please enter a valid number.");
                return;
            }
            if (accountNumber.Length != 8)
            {
                MessageBox.Show("Account number must be 8 digits long.");
                return;
            }
            if (sortCode.Length != 6)
            {
                MessageBox.Show("Sort code must be 6 digits long.");
                return;
            }

            DateTime date = DateTime.Now;

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO accounts (accounttype, institutionname, accountnickname, accountnumber, sortcode, reference, balance, interestrate, owner, createdat)
                                         VALUES (@AccountType, @InstitutionName, @AccountNickname, @AccountNumber, @SortCode, @Reference, @Balance, @InterestRate, @Owner, @CreatedAt)";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@InstitutionName", institutionName);
                            command.Parameters.AddWithValue("@AccountType", accountType);
                            command.Parameters.AddWithValue("@AccountNickname", accountName);
                            command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            command.Parameters.AddWithValue("@SortCode", sortCode);
                            command.Parameters.AddWithValue("@Reference", reference);
                            command.Parameters.AddWithValue("@Balance", startingBalance);
                            command.Parameters.AddWithValue("@InterestRate", interestRate);
                            command.Parameters.AddWithValue("@Owner", owner);
                            command.Parameters.AddWithValue("@CreatedAt", date);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Error adding account: " + ex.Message);
            }
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO transactions (transactionsum, transactiontime, balanceafter, balanceprior, logtype)
                        VALUES (@sum, @time, @accountFK, @balanceafter, @balanceprior, @logtype)";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@sum", startingBalance);
                            command.Parameters.AddWithValue("@time", date);
                            command.Parameters.AddWithValue("@balanceafter", startingBalance);
                            command.Parameters.AddWithValue("@balanceprior", startingBalance);
                            command.Parameters.AddWithValue("@logtype", "Creation");
                            command.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Account added successfully!");
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Error inserting creation transaction: " + ex.Message);
            }
        }

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
