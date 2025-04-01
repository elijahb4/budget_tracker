using IndividualProjectInitial;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            LoadComboBox();
        }

        private void LoadComboBox()
        {
            AccountTypes = GetComboBoxOptions();
            if (AccountTypes.Count == 0)
            {
                MessageBox.Show("No options found for the ComboBox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AccountTypeComboBox.ItemsSource = AccountTypes;
                MessageBox.Show($"ComboBox loaded with {AccountTypes.Count} options.");
                foreach (var type in AccountTypes)
                {
                    MessageBox.Show($"Loaded option: {type}");
                }
            }
        }

        public List<string> GetComboBoxOptions()
        {
            List<string> accountOptions = new List<string>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT Type FROM account_type";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string type = reader.GetString(0);
                                    accountOptions.Add(type);
                                    Console.WriteLine($"Added option: {type}");
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

        public string selectedAccountType { get; set; }

        private void AccountTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountTypeComboBox.SelectedItem != null)
            {
                selectedAccountType = AccountTypeComboBox.SelectedItem?.ToString() ?? string.Empty;
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int owner = GetLoginOwner();
            string accountType = selectedAccountType;

            Console.WriteLine("Account Type to be inserted: " + accountType);

            if (string.IsNullOrEmpty(accountType))
            {
                MessageBox.Show("Please select an account type.");
                return;
            }

            string institutionName = institutionNameTextBox.Text;
            string accountName = accountNameTextBox.Text;
            string accountNumber = accountNumberTextBox.Text;
            string sortCode = sortCodeTextBox.Text;
            string iban = ibanTextBox.Text;
            string bic = bicTextBox.Text;
            string reference = referenceTextBox.Text;
            string startingBalance = balanceTextBox.Text;
            DateTime date = DateTime.Now;

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO liquid_accounts (AccountType, InstitutionName, AccountNickname, AccountNumber, SortCode, IBAN, BIC, Reference, Balance, Owner, CreatedAt)
                        VALUES (@AccountType, @InstitutionName, @AccountNickname, @AccountNumber, @SortCode, @IBAN, @BIC, @Reference, @Balance, @Owner, @CreatedAt)";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@InstitutionName", institutionName);
                            command.Parameters.AddWithValue("@AccountType", accountType);
                            command.Parameters.AddWithValue("@AccountNickname", accountName);
                            command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            command.Parameters.AddWithValue("@SortCode", sortCode);
                            command.Parameters.AddWithValue("@IBAN", iban);
                            command.Parameters.AddWithValue("@BIC", bic);
                            command.Parameters.AddWithValue("@Reference", reference);
                            command.Parameters.AddWithValue("@Balance", startingBalance);
                            command.Parameters.AddWithValue("@Owner", owner);
                            command.Parameters.AddWithValue("@CreatedAt", date);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Account added successfully!");
            }
            catch (MySqlException ex)
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
