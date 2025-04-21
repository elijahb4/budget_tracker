using Mysqlx.Crud;
using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Individual_project_initial
{
    public partial class Home : Page
    {
        public Home()
        {
            this.InitializeComponent();
            DateTime lastRun = Properties.Settings.Default.InterestLastUpdated;
            if ((DateTime.Now - lastRun).TotalDays >= 5)
            {
                UpdateInterest();
            }            
        }
        private void UpdateInterest()
        {
            int owner = GetLoginOwner();
            List<Int32> accounts = new List<Int32>();
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT AccouuntPK FROM accounts WHERE Owner = @Owner";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int accountPK = reader.GetInt32(0);
                                    accounts.Add(accountPK);
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
            foreach (Int32 AccountFK in accounts)
            {
                Insights.InterestEarned(AccountFK);
            }
            Properties.Settings.Default.InterestLastUpdated = DateTime.Now;
            Properties.Settings.Default.Save();
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
