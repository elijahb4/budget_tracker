using Individual_project_initial;
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
    public partial class ViewTargets : Page
    {
        private List<Target> targetOptions = new List<Target>();

        public ViewTargets()
        {
            InitializeComponent();
            int owner = GetLoginOwner();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"SELECT targetpk, ownerfk, accountfk, ""type"", amount, startdate, targetdate, note FROM public.targets WHERE ownerfk = @owner";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@owner", owner);
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
                                    targetOptions.Add(target);
                                }
                            }
                        }
                    }
                }

                foreach (var target in targetOptions)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"Target Type: {target.TargetType}\n" +
                               $"Amount: {target.TargetAmount:C}\n" +
                               $"Start Date: {target.StartDate:d}\n" +
                               $"End Date: {target.EndDate:d}\n" +
                               $"Note: {target.Note}",
                        TextWrapping = TextWrapping.Wrap
                    };

                    TargetStackPanel.Children.Add(textBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading targets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetTargetButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("Targets.xaml", UriKind.Relative));
        }

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
