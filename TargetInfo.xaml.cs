using Npgsql;
using Npgsql;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Individual_project_initial
{
    public partial class TargetInfo : Page
    {
        public int? TargetId { get; private set; }

        public TargetInfo()
        {
            InitializeComponent();
            Loaded += TargetInfo_Loaded;
        }

        private void TargetInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.Source != null)
            {
                var uri = NavigationService.Source;
                var query = uri.OriginalString.Split('?').Skip(1).FirstOrDefault();
                if (!string.IsNullOrEmpty(query))
                {
                    foreach (var part in query.Split('&'))
                    {
                        var kv = part.Split('=');
                        if (kv.Length == 2 && kv[0] == "targetId" && int.TryParse(kv[1], out int id))
                        {
                            TargetId = id;
                            LoadTargetInfo(id);
                        }
                    }
                }
            }
        }

        private void LoadTargetInfo(int TargetId)
        {
            try
            {
                using (var dbHelper = new DatabaseHelper())
                using (var connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT targetpk, ownerfk, accountfk, type, amount, startdate, targetdate, note FROM targets WHERE targetpk = @targetpk";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@targetpk", TargetId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Target target = new Target
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
                                    Text = $"Target Type: {target.TargetType}\n" +
                                        $"Amount: {target.TargetAmount:C}\n" +
                                        $"Start Date: {target.StartDate:d}\n" +
                                        $"End Date: {target.EndDate:d}\n" +
                                        $"Note: {target.Note}",
                                    TextWrapping = TextWrapping.Wrap
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Query Target

        //Query Account

        // Generate Chart
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
            NavigationService?.Navigate(new Uri("ViewTargets.xaml", UriKind.Relative));
        }
    }
}
