using Npgsql;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Mysqlx.Crud;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Media.Effects;

namespace Individual_project_initial
{
    public partial class AccountInformation : Page
    {
        public PlotModel BalanceChart { get; set; }

        public AccountInformation()
        {
            InitializeComponent();
            Loaded += AccountInformation_Loaded;
            LoadChart();
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
                        string query = @"SELECT ""AccountPK"", ""AccountNickname"", ""InstitutionName"", ""Balance"" FROM accounts WHERE ""Owner"" = @owner";
                        //change query
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
                                        InterestRate = reader.GetDecimal(11),
                                        Balance = reader.GetDecimal(12),
                                        AccountType = reader.GetString(13)
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
                        Text = $"Account: {account.AccountNickname}\n Institution: {account.InstitutionName}\n Balance: £{account.Balance}",
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

        public PlotModel LoadChart()
        {
            var BalanceChart = new PlotModel { Title = "Daily Closing Balances" };

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
            series.Points.AddRange(dataPoints);

            BalanceChart.Series.Add(series);
            return BalanceChart;
        }

        public List<DataPoint> GetDailyBalancesFromPostgres()
        {
            var dataPoints = new List<DataPoint>();
            using (var dbHelper = new DatabaseHelper())
            {
                using (var connection = dbHelper.GetConnection())
                {

                    string query = @"SELECT DISTINCT ON (DATE(""Timestamp"")) DATE(""Timestamp"") AS Day, ""Balance"" FROM ""transactions"" WHERE ""Timestamp"" >= @FromDate AND ""Timestamp"" < @ToDate ORDER BY DATE(""Timestamp""), ""Timestamp"" DESC;";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        cmd.Parameters.AddWithValue("@ToDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@FromDate", DateTime.Now.AddDays(-30));
                        while (reader.Read())
                        {
                            DateTime date = reader.GetDateTime(0);
                            double balance = reader.GetDouble(1);

                            // Use DateTimeAxis if you want actual dates on the X axis
                            dataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(date), balance));
                        }
                    }
                }
            }
            return dataPoints;
        }
    }
}