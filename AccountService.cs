using MySql.Data.MySqlClient;
using System;

namespace Individual_project_initial.Services
{
    class AccountService
    {
            private DateTime GetLastUpdated(int AccountPK)
            {
                try
                {
                    string query = "SELECT BalanceLastUpdated FROM liquid_accounts WHERE AccountPK = @AccountPK";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountPK", AccountPK);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read()) ;
                            {
                                DateTime LastUpdated = reader.GetDateTime(0);
                                if (LastUpdated < DateTime.Now)
                                {
                                    updateBalance(AccountPK);
                                }
                                else
                                {
                                    Console.WriteLine("Balance is up to date");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating balance: {ex.Message}");
                }
            }
            private void updateBalance(int AccountPK)
            {
                string updateQuery = "UPDATE liquid_accounts SET Balance = (SELECT SUM(Amount) FROM liquid_transactions WHERE AccountPK = @AccountPK) WHERE AccountPK = @AccountPK";
                using (var updateCommand = new MySqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@AccountPK", AccountPK);
                    updateCommand.ExecuteNonQuery();
                }
            }
    }
}