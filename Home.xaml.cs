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
                // Example: calculate for the last year
                DateTime start = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime end = DateTime.Now;
                InterestEarned(AccountPK, start, end);
                totalInterestEarned += GetTotalInterest(AccountPK);
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
            decimal totalInterestEarned = 0;
            List<Transactionchange> transactions = new List<Transactionchange>();
            decimal interestRate = 0;

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT interestrate FROM accounts WHERE accountpk = @AccountFK";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@AccountFK", AccountFK);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    interestRate = reader.GetDecimal(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving interest rate: " + ex.Message);
            }

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
                                        Timestamp = reader.GetDateTime(3),
                                        BalanceAfter = reader.GetDecimal(4),
                                        BalanceBefore = reader.GetDecimal(5),
                                        LogType = reader.GetString(6),
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

            balance = GetBalanceBeforeDate(AccountFK, startDate);

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                while (txIndex < transactions.Count && transactions[txIndex].Timestamp.Date == date.Date)
                {
                    balance += transactions[txIndex].TransactionSum;
                    txIndex++;
                }
                dailyInterest = balance * (interestRate / 100 / 365);
                totalInterestEarned += dailyInterest;
                decimal balanceafter = balance + dailyInterest;
                InsertInterest(AccountFK, dailyInterest, date, balanceafter, balance);
                balance = balanceafter;
            }

            return totalInterestEarned;
        }
        private void InsertInterest(int AccountFK, decimal transactionSum, DateTime timestamp, decimal balanceafter, decimal balanceprior)
        {
            string logType = "Interest";
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "INSERT INTO transactions (accountfk, transactionsum, transactiontime, balanceafter, balanceprior, logtype) " +
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
                    using var cmd = new NpgsqlCommand(@"SELECT COALESCE(SUM(transactionsum), 0) FROM public.transactions WHERE accountfk = @AccountFK AND logtype = @logtype;", connection);
                    {
                        cmd.Parameters.AddWithValue("@accountfk", accountId);
                        cmd.Parameters.AddWithValue("@logtype", "Interest");

                        var result = cmd.ExecuteScalar();
                        return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
                    }
                }
            }
        }
        private decimal GetBalanceBeforeDate(int AccountFK, DateTime startDate)
        {
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT balanceafter 
                        FROM transactions 
                        WHERE accountfk = @AccountFK 
                        AND transactiontime < @StartDate 
                        ORDER BY transactiontime DESC LIMIT 1";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountFK", AccountFK);
                        command.Parameters.AddWithValue("@StartDate", startDate);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetDecimal(0);
                            }
                        }
                    }

                    query = "SELECT balance FROM accounts WHERE accountpk = @AccountFK";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountFK", AccountFK);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetDecimal(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving balance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return 0;
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
