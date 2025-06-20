using Individual_project_initial;
using Npgsql;
using OxyPlot.Axes;
using OxyPlot;
using System.Windows.Controls;
using System.Windows;
using OxyPlot.Series;

namespace Individual_project_initial
{
    public partial class TargetInfo : Page
    {
        public int? TargetId { get; private set; }
        public PlotModel BalanceChart { get; set; }
        private Target currentTarget;
        private List<Transactionchange> transactionDetails = new List<Transactionchange>();

        public TargetInfo()
        {
            InitializeComponent();
            BalanceChart = new PlotModel { Title = "Target Progress" };
            DataContext = this;
            Loaded += TargetInfo_Loaded;
        }

        private void LoadTargetInfo(int targetId)
        {
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT targetpk, ownerfk, accountfk, type, amount, startdate, targetdate, note 
                               FROM targets WHERE targetpk = @targetpk";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@targetpk", targetId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentTarget = new Target
                                {
                                    TargetId = reader.GetInt32(0),
                                    OwnerId = reader.GetInt32(1),
                                    AccountFK = int.TryParse(reader.GetString(2), out int accFk) ? accFk : 0,
                                    TargetType = reader.GetString(3),
                                    TargetAmount = reader.GetDecimal(4),
                                    StartDate = reader.GetDateTime(5),
                                    EndDate = reader.GetDateTime(6),
                                    Note = reader.IsDBNull(7) ? null : reader.GetString(7)
                                };

                                TextBlock textBlock = new TextBlock
                                {
                                    Text = $"Target Type: {currentTarget.TargetType}\n" +
                                        $"Amount: {currentTarget.TargetAmount:C}\n" +
                                        $"Start Date: {currentTarget.StartDate:d}\n" +
                                        $"End Date: {currentTarget.EndDate:d}\n" +
                                        $"Note: {currentTarget.Note}",
                                    TextWrapping = TextWrapping.Wrap
                                };
                                TargetStackPanel.Children.Add(textBlock);

                                // Query transactions and load chart after target is loaded
                                QueryTransactions(currentTarget);
                                LoadChart();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading target details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QueryTransactions(Target target)
        {
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT transactionpk, accountfk, transactionsum, transactiontime, balanceprior, balanceafter 
                               FROM transactions 
                               WHERE accountfk = @accountfk 
                               AND transactiontime BETWEEN @startDate AND @endDate
                               ORDER BY transactiontime";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@accountfk", target.AccountFK);
                        command.Parameters.AddWithValue("@startDate", target.StartDate);
                        command.Parameters.AddWithValue("@endDate", target.EndDate);

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
                                };
                                transactionDetails.Add(transaction);
                            }
                        }
                    }
                }

                // Add transactions to UI
                foreach (var transaction in transactionDetails)
                {
                    TextBlock transactionBlock = new TextBlock
                    {
                        Text = $"Transaction Date: {transaction.Timestamp:d}\n" +
                              $"Amount: {transaction.TransactionSum:C}\n" +
                              $"Balance After: {transaction.BalanceAfter:C}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    TransactionStackPanel.Children.Add(transactionBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<DataPoint> GetDailyBalancesFromPostgres()
        {
            var dataPoints = new List<DataPoint>();
            if (currentTarget == null) return dataPoints;

            using (var dbHelper = new DatabaseHelper())
            using (var connection = dbHelper.GetConnection())
            {
                string query = @"SELECT day, balanceafter 
                           FROM (
                               SELECT CAST(transactiontime AS DATE) AS day, 
                                      balanceafter, 
                                      ROW_NUMBER() OVER(PARTITION BY CAST(transactiontime AS DATE)
                                          ORDER BY transactiontime DESC) AS rn 
                               FROM transactions 
                               WHERE transactiontime >= @FromDate 
                               AND transactiontime <= @ToDate 
                               AND accountfk = @AccountId
                           ) AS t 
                           WHERE rn = 1 
                           ORDER BY day;";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", currentTarget.StartDate);
                    cmd.Parameters.AddWithValue("@ToDate", currentTarget.EndDate);
                    cmd.Parameters.AddWithValue("@AccountId", currentTarget.AccountFK);

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
    }
}