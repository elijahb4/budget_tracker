using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Google.Protobuf.WellKnownTypes;
using Mysqlx.Crud;
using Npgsql;
using Org.BouncyCastle.Security;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Individual_project_initial
{
    public partial class Insights
    {
        public decimal InterestEarned(int AccountFK, DateOnly startDate, DateOnly endDate)
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
                        string query = "INSERT INTO transactions (AccountFK, TransactionSum, TransactionTime, balanceafter, balanceprior, logtype) " +
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
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }

        public static decimal GetTotalInterest(int accountId)
        {
            using (var dbHelper = new DatabaseHelper())
            {
                using (var connection = dbHelper.GetConnection())
                {
                    using var cmd = new NpgsqlCommand(@"SELECT COALESCE(SUM(""TransactionSum""), 0) FROM public.transactions WHERE ""AccountFK"" = @AccountFK AND logtype = @LogType;", connection);
                    {
                        cmd.Parameters.AddWithValue("@AccountFK", accountId);
                        cmd.Parameters.AddWithValue("@LogType", "interest");

                        var result = cmd.ExecuteScalar();
                        return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
                    }
                }
            }
        }

        public static void SaveInterestToUserSettings(decimal interest)
        {
            Properties.Settings.Default.InterestForYear = interest;
            Properties.Settings.Default.Save();
        }
    }

    class Transactionchange
    {
        public int TrabsactionId { get; set; }
        public int AccountFK { get; set; }
        public decimal TransactionSum { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal BalanceBefore { get; set; }
        public string LogType { get; set; }
        public decimal NewRate { get; set; }
        public decimal RatePrior { get; set; }
    }
}
