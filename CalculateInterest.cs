using Mysqlx.Crud;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Individual_project_initial
{
    internal class CalculateInterest
    {
        private List<Account> Earnings(int userpk)
        {
            var accounts = new List<Account>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"SELECT accountpk, accountnickname, institutionname, accountnumber, sortcode, reference, interestrate, balance, accounttype 
                                       FROM accounts WHERE user_pk = @userpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userpk", userpk);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Account account = new Account
                                    {
                                        AccountPK = reader.GetInt32(0),
                                        AccountNickname = reader.GetString(1),
                                        InstitutionName = reader.GetString(2),
                                        AccountNumber = reader.GetString(3),
                                        SortCode = reader.GetString(4),
                                        Reference = reader.GetString(5),
                                        InterestRate = reader.GetDecimal(6),
                                        Balance = reader.GetDecimal(7),
                                        CreatedAt = reader.GetDateTime(8),
                                        AccountType = reader.GetString(9)
                                    };
                                    accounts.Add(account);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new List<Account>();
            }
            QueryTransactions(accounts);
            return accounts;
        }

        private (List<Transactionchange> Transactions, Dictionary<int, List<int>> DaysBetween) QueryTransactions(List<Account> accounts)
        {
            List<Transactionchange> allTransactions = new List<Transactionchange>();

            foreach (var account in accounts)
            {
                try
                {
                    using (var dbHelper = new DatabaseHelper())
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"SELECT transactionpk, accountfk, transactionsum, transactiontime, balanceprior, balanceafter, logtype
                                       FROM transactions WHERE accountfk = @accountfk ORDER BY transactiontime ASC";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@accountfk", account.AccountPK);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    DateTime transactionTime = reader.GetDateTime(3);

                                    if (transactionTime < account.CreatedAt)
                                        continue;

                                    if (transactionTime > DateTime.Now)
                                        continue;

                                    Transactionchange transaction = new Transactionchange
                                    {
                                        TransactionId = reader.GetInt32(0),
                                        AccountFK = reader.GetInt32(1),
                                        TransactionSum = reader.GetDecimal(2),
                                        Timestamp = transactionTime,
                                        BalanceBefore = reader.GetDecimal(4),
                                        BalanceAfter = reader.GetDecimal(5),
                                        LogType = reader.GetString(6)
                                    };
                                    allTransactions.Add(transaction);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        public decimal CalculateDailyInterestForAccounts(int userPk)
        {
            decimal totalInterestEarned = 0;
            var accounts = Earnings(userPk);
            
            foreach (var account in accounts)
            {
                totalInterestEarned += CalculateDailyInterestForAccount(account);
            }

            return totalInterestEarned;
        }

        private decimal CalculateDailyInterestForAccount(Account account)
        {
            decimal totalInterestEarned = 0;
            var transactions = QueryTransactions(new List<Account> { account })
                .OrderBy(t => t.Timestamp)
                .ToList();

            if (!transactions.Any())
                return 0;

            DateTime startDate = account.CreatedAt.Date;
            DateTime endDate = DateTime.Now.Date;

            var dailyClosingBalances = new Dictionary<DateTime, decimal>();

            decimal currentBalance = account.Balance;

            var transactionsByDate = transactions
                .GroupBy(t => t.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Timestamp).Last());

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (transactionsByDate.TryGetValue(date, out var lastTransaction))
                {
                    currentBalance = lastTransaction.BalanceAfter;
                }

                decimal dailyInterest = CalculateInterestForDay(currentBalance, account.InterestRate);
                totalInterestEarned += dailyInterest;

                if (dailyInterest > 0)
                {
                    RecordInterestTransaction(account.AccountPK, dailyInterest, date, currentBalance);
                    currentBalance += dailyInterest;
                }
            }

            return totalInterestEarned;
        }

        private decimal CalculateInterestForDay(decimal balance, decimal annualInterestRate)
        {
            decimal dailyInterest = balance * (annualInterestRate / 100m / 365m);
            return Math.Round(dailyInterest, 2);
        }

        private void RecordInterestTransaction(int accountPk, decimal interestAmount, DateTime date, decimal currentBalance)
        {
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"INSERT INTO transactions 
                        (accountfk, transactionsum, transactiontime, balanceprior, balanceafter, logtype) 
                        VALUES (@accountfk, @transactionsum, @transactiontime, @balanceprior, @balanceafter, @logtype)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@accountfk", accountPk);
                        command.Parameters.AddWithValue("@transactionsum", interestAmount);
                        command.Parameters.AddWithValue("@transactiontime", date);
                        command.Parameters.AddWithValue("@balanceprior", currentBalance);
                        command.Parameters.AddWithValue("@balanceafter", currentBalance + interestAmount);
                        command.Parameters.AddWithValue("@logtype", "Interest");
                        
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error appropriately
            }
        }

        private bool HasInterestForDate(int accountPk, DateTime date)
        {
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT COUNT(*) FROM transactions 
                                   WHERE accountfk = @accountfk 
                                   AND DATE(transactiontime) = @date 
                                   AND logtype = 'Interest'";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@accountfk", accountPk);
                        command.Parameters.AddWithValue("@date", date.Date);
                        
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
