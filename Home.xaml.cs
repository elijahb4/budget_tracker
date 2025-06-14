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
                totalInterestEarned =+ GetTotalInterest(AccountPK);
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
            decimal least_concern = tax_allowance / 2;
            decimal moderate_concern = tax_allowance * Convert.ToDecimal(0.8);
            decimal most_concern = tax_allowance * Convert.ToDecimal(1.25);
            TextBlock InterestStatus = new TextBlock();
            if (totalInterestEarned < least_concern)
            {
                InterestStatus.Text = $"You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned > least_concern)
            {
                InterestStatus.Text = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's more than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned > moderate_concern && totalInterestEarned <= least_concern)
            {
                InterestStatus.Text = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 80% of your tax-free limit. ";
            }
            else if (totalInterestEarned > moderate_concern && totalInterestEarned < most_concern)
            {
                InterestStatus.Text = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 80% of your tax-free limit. ";
            }
            else
            {
                InterestStatus.Text = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            InterestStackPanel.Children.Add(InterestStatus);
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }

        public decimal InterestEarned(int AccountFK, DateTime startDate, DateTime endDate)
        {
            int owner = GetLoginOwner();
            //update to work monthly, do months where transactions exist and only up to the start if the prior fiscal year and calculations haven't already been made and interest != 0
            decimal totalInterestEarned = 0;
            List<Transactionchange> transactions = new List<Transactionchange>();
            //query the database for account information
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT * FROM transactions WHERE AccountFK = @accountFK ORDER BY TransactionTime ASC";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@accountfk", AccountFK);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Transactionchange transaction = new Transactionchange
                                    {
                                        AccountFK = reader.GetInt32(1),
                                        TransactionSum = reader.GetDecimal(2),
                                        Timestamp = new DateTime(3),
                                        BalanceAfter = reader.GetDecimal(4),
                                        BalanceBefore = reader.GetDecimal(5),
                                        LogType = reader.GetString(6),
                                        NewRate = reader.GetDecimal(7),
                                        RatePrior = reader.GetDecimal(8),
                                    };
                                    transactions.Add(transaction);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving transactions: " + ex.Message);
            }
            DateTime workingDate = new DateTime();
            int txIndex = 0;
            decimal balance = 0;
            decimal dailyInterest = 0;
            decimal balanceprior = 0;
            decimal interestRate = 0;
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Apply any transactions that happened on this day
                while (txIndex < transactions.Count && transactions[txIndex].Timestamp.Date == date.Date)
                {
                    balance += transactions[txIndex].TransactionSum;
                    interestRate = transactions[txIndex].NewRate;
                    txIndex++;
                }

                // Calculate daily interest
                dailyInterest = balance * (interestRate / 100 / 365);

                decimal balanceafter = balanceprior + dailyInterest;
                // Insert the interest for this day
                InsertInterest(AccountFK, dailyInterest, date, balanceafter, balanceprior);
            }
            return totalInterestEarned;
        }
        private void InsertInterest(int AccountFK, decimal transactionSum, DateTime timestamp, decimal balanceafter, decimal balanceprior)
        {
            string logType = "InterestPayment";
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "INSERT INTO transactions (accountfk, amount, transactionsum, balanceafter, balanceprior, logtype) " +
                            "VALUES (@AccountFK, @TransactionSum, @TransactionTime, @balanceafter, @balanceprior, @logtype)";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@AccountFK", AccountFK);
                            command.Parameters.AddWithValue("@TransactionSum", transactionSum);
                            command.Parameters.AddWithValue("@TransactionTime", timestamp);
                            command.Parameters.AddWithValue("@balanceafter", balanceafter);
                            command.Parameters.AddWithValue("@balanceprior", balanceprior);
                            command.Parameters.AddWithValue("@logtype", logType);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting interest: " + ex.Message);
            }
        }

        public static decimal GetTotalInterest(int accountId)
        {
            using (var dbHelper = new DatabaseHelper())
            {
                using (var connection = dbHelper.GetConnection())
                {
                    using var cmd = new NpgsqlCommand(@"SELECT COALESCE(SUM(transactionsum), 0) FROM public.transactions WHERE accountfk = @AccountFK AND logtype = @LogType;", connection);
                    {
                        cmd.Parameters.AddWithValue("@accountfk", accountId);
                        cmd.Parameters.AddWithValue("@logtype", "interest");

                        var result = cmd.ExecuteScalar();
                        return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
                    }
                }
            }
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
        public decimal NewRate { get; set; }
        public decimal RatePrior { get; set; }
        public string Reference { get; internal set; }
    }
}
