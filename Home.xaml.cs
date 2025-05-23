using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Individual_project_initial
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
            UpdateInterest();
        }
        private void UpdateInterest()
        {
            decimal totalInterestEarned = 0;
            int owner = GetLoginOwner();
            List<Int32> accounts = new List<Int32>();
            List<decimal> balances = new List<decimal>();
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT accountpk, balance, accounttype FROM accounts WHERE owner = @Owner";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int accountPK = reader.GetInt32(0);
                                    accounts.Add(accountPK);
                                    decimal balance = reader.GetDecimal(1);
                                    balances.Add(balance);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving account names when updating interest calculations: " + ex.Message);
            }
            decimal totalBalance = 0;
            foreach (decimal balance in balances)
            {
                totalBalance += balance;
            }
            BalanceTextBlock.Text = "Total Balance: £" + totalBalance.ToString("F2");
            foreach (Int32 AccountPK in accounts)
            {
                totalInterestEarned =+ Insights.GetTotalInterest(AccountPK);
            }
            decimal tax_allowance = 0;
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT tax_allowance FROM user_information WHERE user_pk = @Owner";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tax_allowance = reader.GetDecimal(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving your tax limit: " + ex.Message);
            }
            string InterestStatus;
            decimal least_concern = tax_allowance / 2;
            decimal moderate_concern = tax_allowance * Convert.ToDecimal(0.8);
            decimal most_concern = tax_allowance * Convert.ToDecimal(1.25);
            if (totalInterestEarned < least_concern)
            {
                InterestStatus = $"You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned > least_concern)
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's more than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned > moderate_concern && totalInterestEarned <= least_concern)
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 80% of your tax-free limit. ";
            }
            else if (totalInterestEarned > moderate_concern && totalInterestEarned < most_concern)
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 80% of your tax-free limit. ";
            }
            else
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            MessageBox.Show(InterestStatus, "Interest Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }

        private void GoToAccounts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("Accounts.xaml", UriKind.Relative));
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("AddTransaction.xaml", UriKind.Relative));
        }

        private void GoToTargets_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("ViewTargets.xaml", UriKind.Relative));
        }

        private void GoToReminders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("ViewReminders.xaml", UriKind.Relative));
        }
    }
}
