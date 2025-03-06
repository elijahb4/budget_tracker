using MySql.Data.MySqlClient;
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
        public Accounts()
        {
            InitializeComponent();
            int owner = GetLoginOwner();

            List<Account> accountOptions = new List<Account>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        //connection.Open();
                        string query = "SELECT AccountNickname, InstitutionName, Balance, Currency FROM liquid_accounts WHERE Owner = @owner";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Account account = new Account
                                    {
                                        AccountNickname = reader.GetString(0),
                                        InstitutionName = reader.GetString(1),
                                        Balance = reader.GetDecimal(2),
                                        Currency = reader.GetString(3)
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

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
    public class Account
    {
        public string AccountNickname { get; set; }
        public required string InstitutionName { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
    }
}
