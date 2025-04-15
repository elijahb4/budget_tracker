using Npgsql;
using System;

namespace Individual_project_initial.Services
{
    class AccountService
    {
            private void GetLastUpdated(int AccountPK)
            {
                try
                {
                    string query = "SELECT BalanceLastUpdated FROM liquid_accounts WHERE AccountPK = @AccountPK";
                    using (var dbHelper = new DatabaseHelper())
                    {
                        using (var connection = dbHelper.GetConnection())
                        {
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@AccountPK", AccountPK);
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        DateTime LastUpdated = reader.GetDateTime(0);
                                        if (LastUpdated < DateTime.Now)
                                        {
                                            updateBalance(AccountPK);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating balance: {ex.Message}");
                    //return DateTime.MinValue;
                }
            }
            private void updateBalance(int AccountPK)
            {
                string updateQuery = "UPDATE liquid_accounts SET Balance = (SELECT SUM(Amount) FROM liquid_transactions WHERE AccountPK = @AccountPK) WHERE AccountPK = @AccountPK";
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@AccountPK", AccountPK);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
    }
}