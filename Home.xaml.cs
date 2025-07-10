using IndividualProjectInitial;
using Mysqlx.Crud;
using Npgsql;
using System;
using System.Collections.Generic;
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
            UpdateBalance();
            CalculateInterest();
        }

        private void UpdateBalance()
        {
            decimal totalBalance = 0;
            int owner = GetLoginOwner();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string getAccountsQuery = "SELECT accountpk FROM accounts WHERE owner = @Owner";
                    List<int> accountPKs = new List<int>();

                    using (var getAccountsCmd = new NpgsqlCommand(getAccountsQuery, connection))
                    {
                        getAccountsCmd.Parameters.AddWithValue("@Owner", owner);
                        using (var reader = getAccountsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                accountPKs.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    foreach (var accountPK in accountPKs)
                    {
                        decimal latestBalance = 0;
                        string getLatestBalanceQuery = @"
                            SELECT balanceafter 
                            FROM transactions 
                            WHERE accountfk = @AccountPK 
                            ORDER BY transactiontime DESC 
                            LIMIT 1";

                        using (var getLatestBalanceCmd = new NpgsqlCommand(getLatestBalanceQuery, connection))
                        {
                            getLatestBalanceCmd.Parameters.AddWithValue("@AccountPK", accountPK);
                            var result = getLatestBalanceCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                latestBalance = Convert.ToDecimal(result);
                            }
                        }

                        string updateAccountBalanceQuery = "UPDATE accounts SET balance = @Balance WHERE accountpk = @AccountPK";
                        using (var updateAccountCmd = new NpgsqlCommand(updateAccountBalanceQuery, connection))
                        {
                            updateAccountCmd.Parameters.AddWithValue("@Balance", latestBalance);
                            updateAccountCmd.Parameters.AddWithValue("@AccountPK", accountPK);
                            updateAccountCmd.ExecuteNonQuery();
                        }

                        totalBalance += latestBalance;
                    }
                }

                BalanceTextBlock.Text = $"Total Balance: £{totalBalance:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving or updating account balances: " + ex.Message);
            }
        }

        private void CalculateInterest()
        {
            int owner = GetLoginOwner();
            decimal totalInterest = 0;
            try
            {
                var interestCalculator = new CalculateInterest();
                totalInterest = interestCalculator.CalculateInterestForUser(owner);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating interest: " + ex.Message);
            }
            UpdateTotalInterestNotice(totalInterest, owner);
        }
        
        private void UpdateTotalInterestNotice(decimal totalInterestEarned, int owner)
        {
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
            decimal least_concern = tax_allowance / 2;
            decimal moderate_concern = tax_allowance * Convert.ToDecimal(0.8);
            decimal most_concern = tax_allowance * Convert.ToDecimal(1.25);
            TextBlock InterestStatus = new TextBlock();

            if (totalInterestEarned < least_concern)
            {
                InterestStatus.Text = $"You have earned £{totalInterestEarned} in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned >= least_concern && totalInterestEarned < moderate_concern)
            {
                InterestStatus.Text = $"You have earned £{totalInterestEarned} in interest this year. \nThat's more than 50% but less than 80% of your tax-free limit.";
            }
            else if (totalInterestEarned >= moderate_concern && totalInterestEarned < tax_allowance)
            {
                InterestStatus.Text = $"You have earned £{totalInterestEarned} in interest this year. \nThat's more than 80% of your tax-free limit.";
            }
            else if (totalInterestEarned >= tax_allowance && totalInterestEarned < most_concern)
            {
                InterestStatus.Text = $"You have earned £{totalInterestEarned} in interest this year. \nYou are above your tax-free limit!";
            }
            else // totalInterestEarned >= most_concern
            {
                InterestStatus.Text = $"You have earned £{totalInterestEarned} in interest this year. \nThat's well above your tax-free limit!";
            }

            InterestStackPanel.Children.Clear();
            InterestStackPanel.Children.Add(InterestStatus);
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

    class Target
    {
        public int TargetId { get; set; }
        public int OwnerId { get; set; }
        public int AccountFK { get; set; }
        public string TargetType { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Note { get; set; }
    }

    public class Reminder
    {
        public int ReminderPK { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public int UserFK { get; set; }
    }

    class Transactionchange
    {
        public int TransactionId { get; set; }
        public int AccountFK { get; set; }
        public decimal TransactionSum { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal BalanceBefore { get; set; }
        public string LogType { get; set; }
        public string Refernece { get; set; }
        public string Reference { get; internal set; }
    }
}
