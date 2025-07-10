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
        public decimal CalculateInterestForUser(int userId)
        {
            var accounts = GetUserAccounts(userId);
            if (!accounts.Any()) return 0;

            foreach (var account in accounts)
            {
                CalculateInterestForAccount(account);
            }

            return GetTotalInterestFromDb(accounts.Select(a => a.AccountPK).ToList());
        }

        private List<Account> GetUserAccounts(int userId)
        {
            var accounts = new List<Account>();
            try
            {
                using var dbHelper = new DatabaseHelper();
                using var connection = dbHelper.GetConnection();
                string query = @"SELECT accountpk, accountnickname, institutionname, accountnumber, 
                               sortcode, reference, interestrate, balance, createdat, accounttype 
                               FROM accounts WHERE owner = @userpk AND interestrate > 0";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@userpk", userId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    accounts.Add(new Account
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
                    });
                }
            }
            catch (Exception)
            {
                return new List<Account>();
            }
            return accounts;
        }

        private decimal CalculateInterestForAccount(Account account)
        {
            var transactions = GetAccountTransactions(account.AccountPK, account.CreatedAt);
            if (!transactions.Any()) return 0;

            decimal totalInterest = 0;
            decimal currentBalance = transactions.First().BalanceBefore;


            DateTime currentDate = account.CreatedAt.Date;

            DateTime endDate = DateTime.Now.Date.AddDays(-7);

            if (currentDate >= endDate) return 0;

            var transactionsByDate = transactions
                .GroupBy(t => t.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Timestamp).Last());

            while (currentDate <= endDate)
            {
                if (!HasInterestForDate(account.AccountPK, currentDate))
                {
                    if (transactionsByDate.TryGetValue(currentDate, out var lastTransaction))
                    {
                        currentBalance = lastTransaction.BalanceAfter;
                    }

                    if (currentBalance > 0)
                    {
                        decimal dailyInterest = CalculateDailyInterest(currentBalance, account.InterestRate);
                        if (dailyInterest > 0)
                        {
                            totalInterest += dailyInterest;
                            RecordInterestTransaction(account.AccountPK, dailyInterest, currentDate, currentBalance);
                            currentBalance += dailyInterest;
                        }
                    }
                }
                currentDate = currentDate.AddDays(1);
            }
            return totalInterest;
        }

        private List<Transactionchange> GetAccountTransactions(int accountPK, DateTime startDate)
        {
            var transactions = new List<Transactionchange>();
            try
            {
                using var dbHelper = new DatabaseHelper();
                using var connection = dbHelper.GetConnection();
                string query = @"SELECT transactionpk, accountfk, transactionsum, transactiontime, 
                               balanceprior, balanceafter, logtype
                               FROM transactions 
                               WHERE accountfk = @accountfk 
                               AND transactiontime >= @startDate 
                               AND transactiontime < @endDate
                               ORDER BY transactiontime ASC";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountfk", accountPK);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", DateTime.Now.Date);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    transactions.Add(new Transactionchange
                    {
                        TransactionId = reader.GetInt32(0),
                        AccountFK = reader.GetInt32(1),
                        TransactionSum = reader.GetDecimal(2),
                        Timestamp = reader.GetDateTime(3),
                        BalanceBefore = reader.GetDecimal(4),
                        BalanceAfter = reader.GetDecimal(5),
                        LogType = reader.GetString(6)
                    });
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed to retrieve transactions for account: " + accountPK);
            }
            return transactions;
        }

        private decimal CalculateDailyInterest(decimal balance, decimal annualRate)
        {
            return Math.Round(balance * (annualRate / 100m / 365m), 2);
        }

        private void RecordInterestTransaction(int accountPk, decimal interestAmount, DateTime date, decimal currentBalance)
        {
            if (HasInterestForDate(accountPk, date))
                return;

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
                throw new Exception("Failed to record interest transaction: " + ex.Message, ex);
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

        private decimal GetTotalInterestFromDb(List<int> accountPKs)
        {
            if (accountPKs == null || accountPKs.Count == 0)
                return 0;

            decimal totalInterest = 0;
            try
            {
                using var dbHelper = new DatabaseHelper();
                using var connection = dbHelper.GetConnection();
                string query = $@"
                SELECT COALESCE(SUM(transactionsum), 0)
                FROM transactions
                WHERE logtype = 'Interest'
                AND accountfk = ANY(@accountfks)";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountfks", accountPKs);

                var result = command.ExecuteScalar();
                if (result != DBNull.Value)
                    totalInterest = Convert.ToDecimal(result);
            }
            catch (Exception)
            {
                // Optionally log or handle error
                return 0;
            }
            return totalInterest;
        }
    }
}
