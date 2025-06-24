using Mysqlx.Crud;
using Npgsql;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Concurrent;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.TimeZoneInfo;

namespace Individual_project_initial
{
    // This page displays detailed information about a specific account such as account number, sort code, etc.
    public partial class AccountInformation : Page
    {
        public PlotModel BalanceChart { get; set; }
        private int _accountPK;

        public AccountInformation()
        {
            InitializeComponent();
            BalanceChart = new PlotModel { Title = "Daily Closing Balances" };
            DataContext = this;
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

        private void LoadAccountInformation(int accountPK)
        {
            _accountPK = accountPK;
            List<Account> accountDetails = new List<Account>();
            List<Transactionchange> transactionDetails = new List<Transactionchange>();

            // Load account details
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT accountpk, accountnickname, institutionname, accountnumber, sortcode, reference, interestrate, balance, accounttype FROM accounts WHERE accountpk = @accountpk";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@accountpk", accountPK);
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
                                    AccountType = reader.GetString(8)
                                };
                                accountDetails.Add(account);
                            }
                        }
                    }
                }

                if (AccountStackPanel == null)
                {
                    MessageBox.Show("AccountStackPanel is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AccountStackPanel.Children.Clear();
                foreach (var account in accountDetails)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"\nAccount: {account.AccountNickname}\nInstitution: {account.InstitutionName}\nBalance: £{ToString(account.Balance)}\nAccount Number: {account.AccountNumber}\nSort Code: {account.SortCode}\nReference: {account.Reference}\nInterest Rate: {account.InterestRate}\nType: {account.AccountType}\n",
                        TextWrapping = TextWrapping.Wrap
                    };
                    AccountStackPanel.Children.Add(textBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Load transaction details
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT transactionpk, accountfk, transactionsum, transactiontime, balanceprior, balanceafter, logtype FROM transactions WHERE accountfk = @accountfk";
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
                                transactionDetails.Add(transaction);
                            }
                        }
                    }
                }

                if (TransactionStackPanel == null)
                {
                    MessageBox.Show("TransactionStackPanel is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                TransactionStackPanel.Children.Clear();
                foreach (var transaction in transactionDetails)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"\nTransaction Sum: {transaction.TransactionSum}\nTransaction Timestamp: {transaction.Timestamp}\nBalance After: £{ToString(transaction.BalanceAfter)}\nBalance Before: £{ToString(transaction.BalanceBefore)}\n=====",
                        TextWrapping = TextWrapping.Wrap
                    };
                    TransactionStackPanel.Children.Add(textBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadChart();
        }

        private string ToString(decimal balance)
        {
            return balance.ToString("N2");
        }

        public void LoadChart()
        {
            BalanceChart.Axes.Clear();
            BalanceChart.Series.Clear();

            BalanceChart.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM dd",
                Title = "Date",
                IntervalType = DateTimeIntervalType.Days,
                MinorIntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid
            });

            BalanceChart.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Balance",
                MajorGridlineStyle = LineStyle.Solid
            });

            var series = new LineSeries
            {
                Title = "Balance",
                MarkerType = MarkerType.Circle
            };

            var dataPoints = GetDailyBalancesFromPostgres();
            if (dataPoints != null)
                series.Points.AddRange(dataPoints);

            BalanceChart.Series.Add(series);

            BalanceChart.InvalidatePlot(true);
        }

        public List<DataPoint> GetDailyBalancesFromPostgres()
        {
            var dataPoints = new List<DataPoint>();

            using (var dbHelper = new DatabaseHelper())
            using (var connection = dbHelper.GetConnection())
            {
                string query = @"SELECT day, balanceafter FROM(SELECT CAST(transactiontime AS DATE) AS day, balanceafter, ROW_NUMBER() OVER( PARTITION BY CAST(transactiontime AS DATE)
                    ORDER BY transactiontime DESC) AS rn FROM transactions WHERE transactiontime >= @FromDate AND transactiontime < @ToDate AND accountfk = @AccountId) AS t WHERE rn = 1 ORDER BY day;";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", DateTime.Now.AddDays(-30));
                    cmd.Parameters.AddWithValue("@ToDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@AccountId", _accountPK);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime date = reader.GetDateTime(0);
                            decimal balance = reader.GetDecimal(1);
                            dataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(date), (double)balance));
                        }
                    }
                }
            }

            return dataPoints;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri($"Accounts.xaml", UriKind.Relative));
        }
    }
}