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
            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT accountpk, accounttype FROM accounts WHERE owner = @Owner";

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
                totalInterestEarned =+ Insights.GetTotalInterest(AccountFK);
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
            string InterestStatus;
            decimal least_concern = tax_allowance / 2;
            decimal moderate_concern = tax_allowance * Convert.ToDecimal(0.8);
            decimal most_concern = tax_allowance * Convert.ToDecimal(1.25);
            if (totalInterestEarned < least_concern)
            {
                InterestStatus = $"You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned > least_concern)
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's more than 50% of your tax-free limit.";
            }
            else if (totalInterestEarned > moderate_concern && totalInterestEarned <= least_concern)
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 80% of your tax-free limit. ";
            }
            else if (totalInterestEarned > moderate_concern && totalInterestEarned < most_concern)
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 80% of your tax-free limit. ";
            }
            else
            {
                InterestStatus = "You have earned £" + Convert.ToString(totalInterestEarned) + "in interest this year. \nThat's less than 50% of your tax-free limit.";
            }
            MessageBox.Show(InterestStatus, "Interest Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
