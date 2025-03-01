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
        public AddAccount()
        {
            InitializeComponent();
            DataContext = this;
            LoadComboBox();
        }

        private void LoadComboBox()
        {
            List<string> options = GetComboBoxOptions();
            if (options.Count == 0)
            {
                MessageBox.Show("No options found for the ComboBox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AccountTypeComboBox.ItemsSource = options;
                Console.WriteLine("ComboBox options loaded successfully.");
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
                                    Console.WriteLine($"Added option: {type}"); // Debug line
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
                selectedAccountType = AccountTypeComboBox.SelectedItem.ToString();
                // Debugging statement
                Console.WriteLine("Selected Account Type: " + selectedAccountType);
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int owner = GetLoginOwner();
            string accountType = selectedAccountType;

            // Debugging statement
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
            string currency = currencyTextBox.Text;

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO liquid_accounts (AccountType, InstitutionName, AccountNickname, AccountNumber, SortCode, IBAN, BIC, Reference, Balance, Owner, Currency)
                        VALUES (@AccountType, @InstitutionName, @AccountNickname, @AccountNumber, @SortCode, @IBAN, @BIC, @Reference, @Balance, @Owner, @Currency)";

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
                            command.Parameters.AddWithValue("@Currency", currency);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                // Optionally, update the UI or show a success message
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