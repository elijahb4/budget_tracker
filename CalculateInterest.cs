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
        private Dictionary<int, decimal> Earnings(int userpk)
        {
            var accountInterestDict = new Dictionary<int, decimal>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT accountpk, interestrate FROM accounts WHERE user_pk = @userpk";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userpk", userpk);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int accountPK = reader.GetInt32(0);
                                    decimal interestRate = reader.GetDecimal(1);
                                    accountInterestDict[accountPK] = interestRate;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<int, decimal>();
            }

            QueryTransactions(accountInterestDict.Keys.ToList());
            return accountInterestDict;
        }

        private List<Transactionchange> QueryTransactions(List<int> accountPKs)
        {
            List<Transactionchange> allTransactions = new List<Transactionchange>();

            foreach (int accountPK in accountPKs)
            {
                try
                {
                    using (var dbHelper = new DatabaseHelper())
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"SELECT transactionpk, accountfk, transactionsum, transactiontime, 
                                       balanceprior, balanceafter, logtype, newrate, rateprior
                                       FROM transactions 
                                       WHERE accountfk = @accountfk 
                                       ORDER BY transactiontime ASC";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@accountfk", accountPK);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Transactionchange transaction = new Transactionchange
                                    {
                                        TransactionId = reader.GetInt32(0),
                                        AccountFK = reader.GetInt32(1),
                                        TransactionSum = reader.GetDecimal(2),
                                        Timestamp = reader.GetDateTime(3),
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

            return allTransactions;
        }
    }
}
