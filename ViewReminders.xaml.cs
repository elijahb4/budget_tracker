using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Individual_project_initial
{
    public partial class ViewReminders : Page
    {
        private List<Reminder> reminderOptions = new List<Reminder>();

        public ViewReminders()
        {
            InitializeComponent();
            int userId = GetLoginOwner();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"SELECT reminderpk, ""date"", note, userfk 
                                         FROM public.reminders 
                                         WHERE userfk = @userId";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userId", userId);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Reminder reminder = new Reminder
                                    {
                                        ReminderPK = reader.GetInt32(0),
                                        Date = reader.GetDateTime(1),
                                        Note = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        UserFK = reader.GetInt32(3)
                                    };
                                    reminderOptions.Add(reminder);
                                }
                            }
                        }
                    }
                }

                foreach (var reminder in reminderOptions)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = $"Reminder Date: {reminder.Date:g}\nNote: {reminder.Note}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    AccountStackPanel.Children.Add(textBlock);

                    Button moreInfoButton = new Button
                    {
                        Content = "More reminder details",
                        Tag = reminder.ReminderPK,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    moreInfoButton.Click += ReminderMoreInfoButton_Click;

                    AccountStackPanel.Children.Add(moreInfoButton);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reminders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReminderMoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int reminderPK)
            {
                NavigationService.Navigate(new Uri($"ReminderInformation.xaml?reminderPK={reminderPK}", UriKind.Relative));
            }
        }

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}
