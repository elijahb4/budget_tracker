using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class Accounts : Page
    {
        private List<Account> accountOptions = new List<Account>();

        public Accounts()
        {
            InitializeComponent();
            int owner = GetLoginOwner();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT AccountPK, AccountNickname, InstitutionName, Balance, Currency FROM liquid_accounts WHERE Owner = @owner";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Account account = new Account
                                    {
                                        AccountPK = reader.GetInt32(0),
                                        AccountNickname = reader.GetString(1),
                                        InstitutionName = reader.GetString(2),
                                        Balance = reader.GetDecimal(3),
                                        Currency = reader.GetString(4)
                                    };
                                    accountOptions.Add(account);
                                }
                            }
                        }
                    }
                }
                foreach (var account in accountOptions)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"Account: {account.AccountNickname}\n Institution: {account.InstitutionName}\n Balance: {account.Balance}\n Currency: {account.Currency}",
                        TextWrapping = TextWrapping.Wrap
                    };
                    AccountStackPanel.Children.Add(textBlock);
                    Button moreInfoButton = new Button
                    {
                        Content = "Full account details",
                        Tag = account.AccountPK,
                    };
                    moreInfoButton.Click += MoreInfoButton_Click;
                    AccountStackPanel.Children.Add(moreInfoButton);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("AddTransaction.xaml", UriKind.Relative));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("AddAccount.xaml", UriKind.Relative));
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag is int accountPK)
            {
                NavigationService.Navigate(new Uri($"AccountInformation.xaml?accountPK={accountPK}", UriKind.Relative));
            }
        }

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
    public class Account
    {
        public required int AccountPK { get; set; }
        public string AccountNickname { get; set; }
        public required string InstitutionName { get; set; }
        public string AccountNumber { get; set; }
        public string SortCode { get; set; }
        public string Reference { get; set; }
        public string IBAN { get; set; }
        public string BIC { get; set; }
        public bool Overdraft { get; set; }
        public decimal OverdraftLimit { get; set; }
        public decimal OverdraftInterestRate { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
    }
}
