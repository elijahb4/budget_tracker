using Npgsql;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
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
    public partial class TargetInfo : Page
    {
        public TargetInfo()
        {
            InitializeComponent();
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
            NavigationService?.Navigate(new Uri("ViewTargets.xaml", UriKind.Relative));
        }
    }
}
