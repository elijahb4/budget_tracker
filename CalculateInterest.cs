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

        private (List<Transactionchange> Transactions, Dictionary<int, List<int>> DaysBetween) QueryTransactionsWithDays(List<Account> accounts)
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

            var daysBetweenDict = GetDaysBetweenTransactions(accounts);

            return (allTransactions, daysBetweenDict);
        }

        private Dictionary<int, List<int>> GetDaysBetweenTransactions(List<Account> accounts)
        {
            var result = new Dictionary<int, List<int>>();

            foreach (var account in accounts)
            {
                var transactions = QueryTransactions(new List<Account> { account })
                    .OrderBy(t => t.Timestamp)
                    .ToList();

                if (transactions.Count < 2)
                    continue; // No pairs to compare

                var daysBetweenList = new List<int>();
                for (int i = 1; i < transactions.Count; i++)
                {
                    var prev = transactions[i - 1];
                    var curr = transactions[i];
                    int daysBetween = (curr.Timestamp.Date - prev.Timestamp.Date).Days;
                    daysBetweenList.Add(daysBetween);
                }

                result[account.AccountPK] = daysBetweenList;
            }

            return result;
        }

        
    }
}
