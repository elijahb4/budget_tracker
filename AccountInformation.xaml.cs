using Npgsql;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;

namespace Individual_project_initial
{
    public partial class AccountInformation : Page
    {
        public AccountInformation()
        {
            InitializeComponent();
            Loaded += AccountInformation_Loaded;
        }

        private void AccountInformation_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.Source != null)
            {
                var query = NavigationService.Source.OriginalString.Split('?').LastOrDefault();
                if (query != null)
                {
                    var parameters = query.Split('&').Select(p => p.Split('=')).ToDictionary(p => p[0], p => p[1]);
                    if (parameters.TryGetValue("accountPK", out string accountPKString) && int.TryParse(accountPKString, out int accountPK))
                    {
                        LoadAccountInformation(accountPK);
                    }
                }
            }
        }

        private void LoadAccountInformation(int AccountPK)
        {
            List<Account> accountDetails = new List<Account>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT AccountPK, AccountNickname, InstitutionName, AccountNumber, SortCode, Reference, IBAN, BIC, Overdraft, OverdraftLimit, OverdraftInterestRate, InterestRate, Balance, Currency FROM liquid_accounts WHERE AccountPK = @AccountPK";

                        using (var command = new NpgsqlCommand(query, connection))
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
                        Text = $"Account: {account.AccountNickname}\n Institution: {account.InstitutionName}\n Balance: {account.Balance} {account.Currency}",
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
    }
}