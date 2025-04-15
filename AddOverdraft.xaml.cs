using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class AddOverdraft : Page
    {
        private readonly int _accountPK;

        public AddOverdraft(int accountPK)
        {
            InitializeComponent();
            _accountPK = accountPK;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(overdraftLimitTextBox.Text) || string.IsNullOrWhiteSpace(overdraftInterestRateTextBox.Text))
            {
                MessageBox.Show("Please enter an overdraft amount.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            int overdraftBool = 1;
            string overdraftLimitInput = overdraftLimitTextBox.Text;
            string overdraftInterestRateInput = overdraftInterestRateTextBox.Text;
            decimal overdraftLimit = decimal.Parse(overdraftLimitInput);
            decimal overdraftInterestRate = decimal.Parse(overdraftInterestRateInput);

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE liquid accounts SET (Overdraft, OverdraftLimit, OverdraftInterestRate) VALUES (@Overdraft, @OverdraftLimit, @OverdraftInterestRate) WHERE AccountPK = @AccountPK";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Overdraft", overdraftBool);
                            command.Parameters.AddWithValue("@OverdraftLimit", overdraftLimit);
                            command.Parameters.AddWithValue("@OverdraftInterestRate", overdraftInterestRate);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding overdraft: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void submitButton_Click_Overdraft(object sender, RoutedEventArgs e)
        {
            string overdraftLimitInput = overdraftLimitTextBox.Text;
            string overdraftInterestRateInput = overdraftInterestRateTextBox.Text;

            if (string.IsNullOrEmpty(overdraftLimitInput) || string.IsNullOrEmpty(overdraftInterestRateInput))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            decimal overdraftLimit = decimal.Parse(overdraftLimitInput);
            decimal overdraftInterestRate = decimal.Parse(overdraftInterestRateInput);

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "UPDATE account SET OverdraftLimit = @OverdraftLimit, OverdraftInterestRate = @OverdraftInterestRate WHERE AccountID = @AccountID";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@OverdraftLimit", overdraftLimit);
                            command.Parameters.AddWithValue("@OverdraftInterestRate", overdraftInterestRate);
                            command.Parameters.AddWithValue("@AccountID", 1);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
