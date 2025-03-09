using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Individual_project_initial
{
    public partial class AccountInformation : Page
    {
        public AccountInformation()
        {
            InitializeComponent();
        }

        public AccountInformation(int AccountPK) : this()
        {
            InitializeComponent();
            List<Account> accountDetails = new List<Account>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT AccountPK, AccountNickname, InstitutionName, AccountNumber, SortCode, Reference, IBAN, BIC, Overdraft, OverdraftLimit, OverdraftInterestRate, InterestRate, Balance, Currency FROM liquid_accounts WHERE AccountPK = @AccountPK";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@AccountPK", AccountPK);
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
                                        IBAN = reader.GetString(6),
                                        BIC = reader.GetString(7),
                                        Overdraft = reader.GetBoolean(8),
                                        OverdraftLimit = reader.GetDecimal(9),
                                        OverdraftInterestRate = reader.GetDecimal(10),
                                        InterestRate = reader.GetDecimal(11),
                                        Balance = reader.GetDecimal(12),
                                        Currency = reader.GetString(13)
                                    };
                                    accountDetails.Add(account);
                                    MessageBox.Show($"Loaded account: {account.AccountNickname}, Balance: {account.Balance}");
                                }
                            }
                        }
                    }
                }
                if (AccountStackPanel == null)
                {
                    MessageBox.Show("AccountStackPanel is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                foreach (var account in accountDetails)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"Account: {account.AccountNickname}\n Institution: {account.InstitutionName}\n Balance: {account.Balance}{account.Currency}",
                        TextWrapping = TextWrapping.Wrap
                    };
                    MessageBox.Show($"Added TextBlock for account: {account.AccountNickname}");
                    AccountStackPanel.Children.Add(textBlock);
                    TextBlock additionalInfoTextBlock = new TextBlock
                    {
                        Text = $"Account Number: {account.AccountNumber}\n Sort Code: {account.SortCode}\n Reference: {account.Reference}\n IBAN: {account.IBAN}\n BIC: {account.BIC}\n Overdraft: {account.Overdraft}\n Overdraft Limit: {account.OverdraftLimit}\n Overdraft Interest Rate: {account.OverdraftInterestRate}\n Interest Rate: {account.InterestRate}",
                    };
                    AccountStackPanel.Children.Add(additionalInfoTextBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
}
